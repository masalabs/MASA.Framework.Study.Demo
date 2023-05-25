using Assignment_Client.Middleware;
using Assignment_Contracts.Requests;
using Assignment_Contracts.Responses;
using Masa.BuildingBlocks.Service.Caller;
using Masa.Contrib.Service.Caller.HttpClient;

namespace Assignment_Client.Callers;

public sealed class TodoServiceCaller : HttpClientCallerBase
{
    protected override string BaseAddress { get; set; } = "https://localhost:7192";

    protected override string Prefix { get; set; } = "todo";

    public Task AddAsync(AddTodoRequest request)
    {
        return Caller.PostAsync("add", request);
    }

    public Task UpdateAsync(EditTodoRequest request)
    {
        return Caller.PutAsync("update", request);
    }

    public Task<List<TodoListItemResponse>?> GetListAsync(
        int page,
        int pageSize,
        string title)
    {
        return Caller.GetAsync<List<TodoListItemResponse>>("items", new
        {
            page,
            pageSize,
            title
        });
    }

    public Task DeleteAsync(int id)
    {
        return Caller.DeleteAsync($"delete/{id}", new { });
    }

    protected override void UseHttpClientPost(MasaHttpClientBuilder masaHttpClientBuilder)
    {
        masaHttpClientBuilder.AddMiddleware<TraceMiddleware>();
    }

    // protected override void ConfigMasaCallerClient(MasaCallerClient callerClient)
    // {
    //     // Use Xml Request (install Masa.Contrib.Service.Caller.Serialization.Xml nuget)
    //     callerClient.UseXml();
    // }
}
