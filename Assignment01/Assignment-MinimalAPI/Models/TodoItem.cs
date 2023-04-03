namespace Assignment_MinimalAPI.Models;

public class TodoItem
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public string? Describe { get; set; }

    public bool Done { get; set; }
}
