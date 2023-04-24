using Assignment_MinimalAPI.Models;
using Assignment_MinimalAPI.Requests;
using System.Diagnostics.CodeAnalysis;

namespace Assignment_MinimalAPI.Services;

public class TodoService : ServiceBase
{
    private readonly List<TodoItem> _data = new()
    {
        new TodoItem()
        {
            Id = 1,
            Title = "Masa Framework入门学习",
            Done = false,
        }
    };

    /// <summary>
    /// 获取日志服务
    /// </summary>
    // private ILogger<TodoService> _logger => GetRequiredService<ILogger<TodoService>>();

    public Task<List<TodoItem>> GetListAsync()
    {
        var list = _data;
        return Task.FromResult(list);
    }

    public Task CreateAsync(CreateTodoRequest request)
    {
        var id = _data.Select(t => t.Id).Max();
        var todoItem = new TodoItem()
        {
            Id = id + 1,
            Title = request.Title
        };
        _data.Add(todoItem);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UpdateTodoRequest request)
    {
        ParseValidate(request.Id, out var todoItem);

        todoItem.Title = request.Title;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        ParseValidate(id, out var todoItem);

        _data.Remove(todoItem);
        return Task.CompletedTask;
    }

    [RoutePattern(HttpMethod = "Post")]
    public Task DoneAsync(int id)
    {
        ParseValidate(id, out var todoItem);

        todoItem.Done = true;
        return Task.CompletedTask;
    }

    [DoesNotReturn]
    void ParseValidate(int id, out TodoItem? todoItem)
    {
        if (!TryValidate(id, out todoItem))
            throw new Exception("不存在的代办事项");
    }

    bool TryValidate(int id, [NotNullWhen(true)] out TodoItem? todoItem)
    {
        todoItem = _data.FirstOrDefault(t => t.Id == id);
        return todoItem != null;
    }
}
