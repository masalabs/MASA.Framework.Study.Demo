namespace Assignment_MinimalAPI.Request;

public class UpdateTodoRequest : CreateTodoRequest
{
    public int Id { get; set; }
}
