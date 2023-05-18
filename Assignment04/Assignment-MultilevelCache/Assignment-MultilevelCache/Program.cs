using Assignment_MasaDbContext.Entities;
using Assignment_MasaDbContext.Infrastructure;
using Assignment_MasaDbContext.Requests;
using Assignment_MasaDbContext.Responses;
using Masa.BuildingBlocks.Caching;
using Masa.BuildingBlocks.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMasaDbContext<TodoDbContext>(dbContextBuilder =>
{
    dbContextBuilder.UseSqlServer();
    dbContextBuilder.UseFilter();
});

// thread safe class

builder.Services.AddSingleton<CustomOptionsCache>();
builder.Services.AddTransient<CustomOptionsManager>();

// Register multilevel cache

builder
    .Services
    .AddMultilevelCache(cacheBuilder =>
    {
        cacheBuilder.UseStackExchangeRedisCache();
    });

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

#region benchmark

app.MapGet("/benchmark/redirect", () =>
{
    return new TodoItemResponse()
    {
        Id = 1,
        Title = "string",
        Describe = "string",
        Done = false
    };
});

app.MapGet("benchmark/multilevel_cache/{id}", async (
    IServiceProvider serviceProvider,
    int id) =>
{
    var multilevelCacheClient = serviceProvider.GetRequiredService<IMultilevelCacheClient>();
    TimeSpan? memoryTimeSpan = null;
    var todoInfo = await multilevelCacheClient.GetOrSetAsync(id.ToString(), async () =>
    {
        var context = serviceProvider.GetService<TodoDbContext>();
        var todoInfo = await context.Set<TodoItem>().Select(t => new TodoItemResponse()
        {
            Id = t.Id,
            Title = t.Title,
            Describe = t.Describe,
            Done = t.Done
        }).FirstOrDefaultAsync(t => t.Id == id);

        if (todoInfo != null)
        {
            memoryTimeSpan = TimeSpan.FromSeconds(300); //Set the memory cache expiration time to expire after 1 minute
            return new CacheEntry<TodoItemResponse>(todoInfo, TimeSpan.FromDays(1)); //Set the distributed cache expiration time to expire after 1 day
        }
        memoryTimeSpan = TimeSpan.FromSeconds(15); //Set the memory cache expiration time to expire after 15 second
        return new CacheEntry<TodoItemResponse>(todoInfo, TimeSpan.FromMinutes(30)); //Set the memory cache expiration time to expire after 30 minutes
    }, memoryCacheOptions =>
    {
        memoryCacheOptions.AbsoluteExpirationRelativeToNow = memoryTimeSpan;
    });
    return todoInfo;
});

app.MapGet("benchmark/distribute_cache/{id}", async (
    IServiceProvider serviceProvider,
    int id) =>
{
    var distributedCacheClient = serviceProvider.GetRequiredService<IDistributedCacheClient>();
    var todoInfo = await distributedCacheClient.GetOrSetAsync(id.ToString(), async () =>
    {
        var context = serviceProvider.GetService<TodoDbContext>();
        var todoInfo = await context.Set<TodoItem>().Select(t => new TodoItemResponse()
        {
            Id = t.Id,
            Title = t.Title,
            Describe = t.Describe,
            Done = t.Done
        }).FirstOrDefaultAsync(t => t.Id == id);

        if (todoInfo != null)
        {
            return new CacheEntry<TodoItemResponse>(todoInfo, TimeSpan.FromDays(1)); //Set the distributed cache expiration time to expire after 1 day
        }
        return new CacheEntry<TodoItemResponse>(todoInfo, TimeSpan.FromMinutes(30)); //Set the memory cache expiration time to expire after 30 minutes
    });
    return todoInfo;
});

app.MapGet("benchmark/database/{id}", async (
    IServiceProvider serviceProvider,
    int id) =>
{
    var context = serviceProvider.GetService<TodoDbContext>();
    var todoInfo = await context.Set<TodoItem>().Select(t => new TodoItemResponse()
    {
        Id = t.Id,
        Title = t.Title,
        Describe = t.Describe,
        Done = t.Done
    }).FirstOrDefaultAsync(t => t.Id == id);

    return todoInfo;
});

#endregion

#region TodoList

app.MapGet("todo/{id}", async (
    [FromServices] TodoDbContext context,
    [FromServices] IMultilevelCacheClient multilevelCacheClient,
    int id) =>
{
    TimeSpan? memoryTimeSpan = null;
    var todoInfo = await multilevelCacheClient.GetOrSetAsync(id.ToString(), async () =>
    {
        var todoInfo = await context.Set<TodoItem>().Select(t => new TodoItemResponse()
        {
            Id = t.Id,
            Title = t.Title,
            Describe = t.Describe,
            Done = t.Done
        }).FirstOrDefaultAsync(t => t.Id == id);

        if (todoInfo != null)
        {
            memoryTimeSpan = TimeSpan.FromSeconds(60); //Set the memory cache expiration time to expire after 1 minute
            return new CacheEntry<TodoItemResponse>(todoInfo, TimeSpan.FromDays(1)); //Set the distributed cache expiration time to expire after 1 day
        }
        memoryTimeSpan = TimeSpan.FromSeconds(15); //Set the memory cache expiration time to expire after 15 second
        return new CacheEntry<TodoItemResponse>(todoInfo, TimeSpan.FromMinutes(30)); //Set the memory cache expiration time to expire after 30 minutes
    }, memoryCacheOptions =>
    {
        memoryCacheOptions.AbsoluteExpirationRelativeToNow = memoryTimeSpan;
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
    [FromServices] IMultilevelCacheClient multilevelCacheClient,
    int id) =>
{
    var todoInfo = await context.Set<TodoItem>().AsTracking().FirstOrDefaultAsync(t => t.Id == id);
    ArgumentNullException.ThrowIfNull(todoInfo);

    context.Set<TodoItem>().Remove(todoInfo);
    await context.SaveChangesAsync();

    await multilevelCacheClient.RemoveAsync<TodoItemResponse>(id.ToString());

    return Results.Accepted();
});

#endregion

#region thread safe class

app.MapGet("/threadsafe", (CustomOptionsManager customOptionsManager) =>
{
    return customOptionsManager.GetValue("ConnectionStrings:DefaultConnection");
});

#endregion

app.Run();
