namespace Assignment_Contracts.Requests;

public class AddTodoRequest
{
    public string Title { get; set; } = default!;
    
    public string? Describe { get; set; }
}
