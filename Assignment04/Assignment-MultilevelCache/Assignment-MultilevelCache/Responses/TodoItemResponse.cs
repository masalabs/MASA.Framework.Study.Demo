namespace Assignment_MasaDbContext.Responses;

public class TodoItemResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;
    
    public string? Describe { get; set; }

    public bool Done { get; set; }
}
