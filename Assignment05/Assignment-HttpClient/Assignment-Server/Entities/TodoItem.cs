namespace Assignment_Server.Entities;

public class TodoItem
{
    public int Id { get; set; }
    
    public string Title { get; set; } = default!;

    public string? Describe { get; set; }

    public bool Done { get; set; }
    
    public DateTime CreateTime { get; set; }
    
    public DateTime UpdateTime { get; set; }
}
