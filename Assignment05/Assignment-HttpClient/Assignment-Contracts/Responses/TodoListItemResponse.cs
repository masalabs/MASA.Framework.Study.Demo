namespace Assignment_Contracts.Responses;

public class TodoListItemResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string? Describe { get; set; }

    public bool Done { get; set; }
}
