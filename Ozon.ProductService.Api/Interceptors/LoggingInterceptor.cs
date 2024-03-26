using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Ozon.ProductService.Api.Interceptors;

public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation($"Starting call. Type/Method: {context.Method.GetType()} | {context.Method}");
        var response = await continuation(request, context);
        logger.LogInformation($"End call. Type/Method: {context.Method.GetType()} | {context.Method}");

        return response;
    }
}