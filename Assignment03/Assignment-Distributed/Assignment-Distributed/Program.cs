using Assignment_MasaDbContext.Entities;
using Assignment_MasaDbContext.Infrastructure;
using Assignment_MasaDbContext.Requests;
using Assignment_MasaDbContext.Responses;
using Masa.BuildingBlocks.Caching;
using Masa.BuildingBlocks.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMasaDbContext<TodoDbContext>(dbContextBuilder =>
{
    dbContextBuilder.UseSqlite();
    dbContextBuilder.UseFilter();
});

// Register distributed cache

builder.Services.AddDistributedCache(cacheBuilder => cacheBuilder.UseStackExchangeRedisCache());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region TodoList

app.MapGet("todo/{id}", async (
    [FromServices] TodoDbContext context,
    [FromServices] IDistributedCacheClient distributedCacheClient,
    int id) =>
{
    var todoInfo = await distributedCacheClient.GetOrSetAsync(id.ToString(), async () =>
    {
        var todoInfo = await context.Set<TodoItem>().Select(t => new TodoItemResponse()
        {
            Id = t.Id,
            Title = t.Title,
            Describe = t.Describe,
            Done = t.Done
        }).FirstOrDefaultAsync(t => t.Id == id);
        return todoInfo != null ?
            new CacheEntry<TodoItemResponse>(todoInfo, TimeSpan.FromMinutes(5))
            : new CacheEntry<TodoItemResponse>(null!, TimeSpan.FromSeconds(30));
    });
    ArgumentNullException.ThrowIfNull(todoInfo);
    return todoInfo;
});

app.MapGet("todo/list", (
    [FromServices] TodoDbContext context,
    int page,
    int pageSize) =>
{
    var list = context
        .Set<TodoItem>()
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => new TodoListItemResponse()
        {
            Id = t.Id,
            Title = t.Title,
            Done = t.Done
        }).ToList();
    return list;
});

app.MapGet("todo/recycle/list", (
    [FromServices] TodoDbContext context,
    [FromServices] IDataFilter dataFilter,
    int page,
    int pageSize) =>
{
    using (dataFilter.Disable<ISoftDelete>())
    {
        var list = context
            .Set<TodoItem>()
            .Where(t => t.IsDeleted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TodoListItemResponse()
            {
                Id = t.Id,
                Title = t.Title,
                Done = t.Done
            }).ToList();
        return list;
    }
});

app.MapPost("todo", async ([FromServices] TodoDbContext context, [FromBody] AddTodoRequest request) =>
{
    var todo = new TodoItem()
    {
        Title = request.Title,
        Describe = request.Describe
    };
    await context.Set<TodoItem>().AddAsync(todo);
    await context.SaveChangesAsync();
    return Results.Accepted();
});

app.MapPut("todo/done/{id}", async ([FromServices] TodoDbContext context, int id) =>
{
    var todoInfo = await context.Set<TodoItem>().AsTracking().FirstOrDefaultAsync(t => t.Id == id);
    ArgumentNullException.ThrowIfNull(todoInfo);

    todoInfo.Done = true;

    context.Set<TodoItem>().Update(todoInfo);
    await context.SaveChangesAsync();
    return Results.Accepted();
});

app.MapDelete("todo/{id}", async (
    [FromServices] TodoDbContext context,
    [FromServices] IDistributedCacheClient distributedCacheClient,
    int id) =>
{
    var todoInfo = await context.Set<TodoItem>().AsTracking().FirstOrDefaultAsync(t => t.Id == id);
    ArgumentNullException.ThrowIfNull(todoInfo);

    context.Set<TodoItem>().Remove(todoInfo);
    await context.SaveChangesAsync();

    await distributedCacheClient.RemoveAsync<TodoItemResponse>(id.ToString());

    // full cache key

    // await distributedCacheClient.RemoveAsync($"{nameof(TodoItemResponse)}.{id}");

    return Results.Accepted();
});

app.MapGet("/init", async (IDistributedCacheClient distributedCacheClient) =>
{
    await distributedCacheClient.SetAsync<long>("goods_1_stock", 3);
});

app.MapGet("/increment", async (IDistributedCacheClient distributedCacheClient) =>
{
    var stock = await distributedCacheClient.HashIncrementAsync("goods_1_stock");
    return $"set success, current stock: {stock}";
});

app.MapGet("/decrement", async (IDistributedCacheClient distributedCacheClient) =>
{
    var stock = await distributedCacheClient.HashDecrementAsync("goods_1_stock");
    if (stock != null)
    {
        return $"set success, current stock: {stock}";
    }
    return "set failed";
});

#endregion

app.Run();
