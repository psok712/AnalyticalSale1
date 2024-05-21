using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain.Interfaces;

namespace ProductService.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IProductRepository, ProductRepository>();
    }
}