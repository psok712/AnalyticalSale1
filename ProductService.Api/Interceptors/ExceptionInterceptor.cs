using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ProductService.Api.Interceptors;

public class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            logger.LogError(ex.StackTrace);
            throw new RpcException(
                new Status(StatusCode.Internal, ex.Message));
        }
    }
}