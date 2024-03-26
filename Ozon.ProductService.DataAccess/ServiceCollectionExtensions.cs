using Microsoft.Extensions.DependencyInjection;
using Ozon.ProductService.Domain.Interfaces;

namespace Ozon.ProductService.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IProductRepository, ProductRepository>();
    }
}