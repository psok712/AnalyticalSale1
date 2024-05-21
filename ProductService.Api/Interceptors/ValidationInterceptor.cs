using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ProductService.Api.Interceptors;

public class ValidationInterceptor(IServiceProvider serviceProvider) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>();
        var validationResult = validator?.Validate(request);

        if (validationResult is null || validationResult.IsValid)
            return continuation(request, context);

        var messageError = string.Join(' ',
            validationResult.Errors.Select(e => e.ErrorMessage));

        throw new RpcException(new Status(StatusCode.InvalidArgument, messageError));
    }
}