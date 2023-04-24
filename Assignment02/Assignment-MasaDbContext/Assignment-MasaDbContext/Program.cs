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
    dbContextBuilder.UseSqlite("Data Source=todo.db;");
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

app.MapGet("get", (
    [FromServices] TodoDbContext context,
    int id) =>
{
    return context.Set<TodoItem>().FirstOrDefaultAsync(t => t.Id == id);
});

app.MapGet("list", (
    TodoDbContext context,
    int page,
    int pageSize) =>
{
    var list = context.Set<TodoItem>().Skip((page - 1) * pageSize).Take(pageSize).ToList();
    return list;
});

app.MapGet("/recycle/list", (
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

app.MapPost("add", async ([FromServices] TodoDbContext context, [FromBody] AddTodoRequest request) =>
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

app.MapDelete("delete", async ([FromServices] TodoDbContext context, [FromBody] DeleteTodoRequest request) =>
{
    var todoInfo = await context.Set<TodoItem>().AsTracking().FirstOrDefaultAsync(t => t.Id == request.Id);
    if (todoInfo == null)
        throw new Exception("该事项不存在");

    context.Set<TodoItem>().Remove(todoInfo);
    await context.SaveChangesAsync();
    return Results.Accepted();
});

app.Run();