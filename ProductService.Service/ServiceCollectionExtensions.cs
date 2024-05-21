using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain.Interfaces;

namespace ProductService.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddScoped<IProductService, ProductService>();
    }
}