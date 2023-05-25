using Masa.BuildingBlocks.Service.Caller;

namespace Assignment_Client.Middleware;

public class TraceMiddleware : ICallerMiddleware
{
    private readonly string _traceId;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public TraceMiddleware(IHttpContextAccessor? httpContextAccessor = null)
    {
        _traceId = "trace-id";
        _httpContextAccessor = httpContextAccessor;
    }

    public Task HandleAsync(MasaHttpContext masaHttpContext, CallerHandlerDelegate next, CancellationToken cancellationToken = default)
    {
        if (!masaHttpContext.RequestMessage.Headers.Contains(_traceId) && _httpContextAccessor?.HttpContext != null)
        {
            masaHttpContext.RequestMessage.Headers.Add(_traceId, _httpContextAccessor.HttpContext.Request.Headers[_traceId].ToString());
        }
        return next();
    }
}
