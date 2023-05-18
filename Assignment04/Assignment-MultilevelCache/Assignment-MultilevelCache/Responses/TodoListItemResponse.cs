namespace Assignment_MasaDbContext.Responses;

public class TodoListItemResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;

    public bool Done { get; set; }
}
