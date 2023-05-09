using Assignment_MasaDbContext.Entities;
using Assignment_MasaDbContext.Infrastructure;
using Assignment_MasaDbContext.Requests;
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

app.MapGet("todo/list", (
    TodoDbContext context,
    int page,
    int pageSize) =>
{
    var list = context.Set<TodoItem>().Skip((page - 1) * pageSize).Take(pageSize).ToList();
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
        var list = context.Set<TodoItem>().Where(t => t.IsDeleted).Skip((page - 1) * pageSize).Take(pageSize).ToList();
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

app.MapDelete("todo/{id}", async ([FromServices] TodoDbContext context, int id) =>
{
    var todoInfo = await context.Set<TodoItem>().AsTracking().FirstOrDefaultAsync(t => t.Id == id);
    ArgumentNullException.ThrowIfNull(todoInfo);

    context.Set<TodoItem>().Remove(todoInfo);
    await context.SaveChangesAsync();
    return Results.Accepted();
});

#endregion

app.Run();
