using Assignment_Client.Callers;
using Assignment_Contracts.Requests;
using Assignment_Contracts.Responses;
using Masa.BuildingBlocks.Service.Caller;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoRegistrationCaller();

// builder.Services.AddCaller(callerBuilder =>
// {
//     callerBuilder.UseHttpClient(client =>
//     {
//         client.BaseAddress = "https://localhost:7192";
//         client.Prefix = "todo";
//
//         // client.UseXml();
//     });
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World, I'm Client!");

#region plan 1

app.MapPost("todo/add", (AddTodoRequest request, TodoServiceCaller caller) => caller.AddAsync(request));

app.MapPut("todo/update", (EditTodoRequest request, TodoServiceCaller caller) => caller.UpdateAsync(request));

app.MapGet("todo/items", (
    int page,
    int pageSize,
    string title,
    TodoServiceCaller caller) => caller.GetListAsync(page, pageSize, title));

app.MapDelete("todo/delete/{id}", (int id, TodoServiceCaller caller) => caller.DeleteAsync(id));

#endregion

#region plan 2

// app.MapPost("todo/add", async (AddTodoRequest request, ICaller caller) =>
// {
//     await caller.PostAsync("add", request);
//     return Results.Accepted();
// });
//
// app.MapPut("todo/update", async (EditTodoRequest request, ICaller caller) =>
// {
//     await caller.PutAsync("update", request);
//     return Results.Accepted();
// });
//
// app.MapGet("todo/items", (
//     int page,
//     int pageSize,
//     string title,
//     ICaller caller) => caller.GetAsync<List<TodoListItemResponse>>("items", new
// {
//     page,
//     pageSize,
//     title
// }));

app.MapDelete("todo/delete/{id}", async (int id, ICaller caller) =>
{
    await caller.DeleteAsync($"delete/{id}", new { });

    return Results.Accepted();
});

#endregion


app.Run();
