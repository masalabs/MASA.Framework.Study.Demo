using System.Linq.Expressions;
using Assignment_Contracts.Requests;
using Assignment_Contracts.Responses;
using Assignment_Server.Entities;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/", () => "Hello World, I'm Server!");

var data = new List<TodoItem>();

app.MapPost("todo/add", (AddTodoRequest request) =>
{
    var todoItem = new TodoItem()
    {
        Id = GetNextId(),
        Title = request.Title,
        Describe = request.Describe,
        CreateTime = DateTime.Now,
        UpdateTime = DateTime.Now
    };
    data.Add(todoItem);
    return Results.Accepted();

    int GetNextId() => data.OrderByDescending(t => t.Id).Select(t => t.Id).FirstOrDefault() + 1;
});

app.MapPut("todo/update", (EditTodoRequest request) =>
{
    var item = data.FirstOrDefault(t => t.Id == request.Id);
    ArgumentNullException.ThrowIfNull(item);

    item.Title = request.Title;
    item.Describe = request.Describe;
    return Results.Accepted();
});

app.MapGet("todo/items", (
    int page,
    int pageSize,
    string title,
    HttpContext httpContext) =>
{
    Console.WriteLine($"current headers: {httpContext.Request.Headers}");
    Expression<Func<TodoItem, bool>> condition = t => true;
    condition = condition.And(!string.IsNullOrWhiteSpace(title), t => t.Title.Contains(title));
    var list = data
        .Where(condition.Compile())
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => new TodoListItemResponse()
        {
            Id = t.Id,
            Title = t.Title,
            Describe = t.Describe,
            Done = t.Done
        }).ToList();
    return Results.Json(list);
});

app.MapDelete("todo/delete/{id}", (int id) =>
{
    var item = data.FirstOrDefault(t => t.Id == id);
    ArgumentNullException.ThrowIfNull(item);

    data.Remove(item);
    return Results.Accepted();
});

app.Run();
