using AutoBogus;
using AutoBogus.Conventions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Ozon.ProductService.DataAccess.Exceptions;
using Ozon.ProductService.Domain.Interfaces;
using Ozon.ProductService.Domain.Models;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Ozon.ProductService.IntegrationTest;

public class CustomDependenciesWebApplicationFactory<TEntryPoint>
    : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    private readonly Mock<IProductRepository> _productRepositoryFake = new(MockBehavior.Strict);
    public readonly List<Product> CorrectListProduct = GenerateValidListProducts(20);

    public CustomDependenciesWebApplicationFactory()
    {
        AutoFaker.Configure(f => f.WithConventions());
        const int expectedCreateProductId = 1;
        const int incorrectProductId = -1;
        var firstProductId = CorrectListProduct.First().Id;

        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(CorrectListProduct);

        _productRepositoryFake
            .Setup(f => f.Add(It.IsAny<Product>()))
            .Returns(expectedCreateProductId);

        _productRepositoryFake
            .Setup(f => f.UpdatePrice(It.IsAny<long>(), It.IsAny<double>()));

        _productRepositoryFake
            .Setup(f => f.Get(firstProductId))
            .Returns(CorrectListProduct.Where(p => p.Id == firstProductId).First);
        _productRepositoryFake
            .Setup(f => f.Get(incorrectProductId))
            .Throws(new NotFoundProductException("No product found with this ID."));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Replace(new ServiceDescriptor(typeof(IProductRepository), _productRepositoryFake.Object));
        });
    }
    
    private static List<Product> GenerateValidListProducts(int count)
    {
        return new AutoFaker<Product>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, faker
                => (CategoryProduct)faker.Random.Number(1, Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate(count);
    }
}