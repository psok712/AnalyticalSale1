using Microsoft.Extensions.DependencyInjection;
using Ozon.ProductService.Domain.Interfaces;

namespace Ozon.ProductService.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddScoped<IProductService, ProductService>();
    }
}