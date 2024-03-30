using AutoBogus;
using AutoBogus.Conventions;
using Bogus;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Ozon.ProductService.Api;
using Ozon.ProductService.DataAccess.Exceptions;
using Ozon.ProductService.Domain.Interfaces;
using Ozon.ProductService.Domain.Models;
using Ozon.ProductService.IntegrationTest.GrpcHelpers;
using Xunit.Abstractions;
using Enum = System.Enum;

namespace Ozon.ProductService.IntegrationTest;

public class ProductServiceTestsGrpc : IntegrationTestBase
{
    private readonly List<Product> _correctListProduct = GenerateValidListProducts(20);
    private readonly Mock<IProductRepository> _productRepositoryFake = new(MockBehavior.Strict);

    public ProductServiceTestsGrpc(
        GrpcTestFixture<Startup> fixture,
        ITestOutputHelper outputHelper
    ) : base(fixture, outputHelper)
    {
        const int expectedCreateProductId = 1;
        const int incorrectProductId = -1;
        var firstProductId = _correctListProduct.First().Id;

        _productRepositoryFake
            .Setup(f => f.Add(It.IsAny<Product>()))
            .Returns(expectedCreateProductId);

        _productRepositoryFake
            .Setup(f => f.UpdatePrice(It.IsAny<long>(), It.IsAny<double>()));

        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_correctListProduct);

        _productRepositoryFake
            .Setup(f => f.Get(firstProductId))
            .Returns(_correctListProduct.Where(p => p.Id == firstProductId).First);
        _productRepositoryFake
            .Setup(f => f.Get(incorrectProductId))
            .Throws(new NotFoundProductException("No product found with this ID."));

        Fixture.ConfigureWebHost(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(new ServiceDescriptor(typeof(IProductRepository),
                    _productRepositoryFake.Object));
            });
        });
    }

    public static IEnumerable<object[]> CategoryFilter()
    {
        yield return [CategoryProduct.General];
        yield return [CategoryProduct.HouseholdChemicals];
        yield return [CategoryProduct.Technique];
        yield return [CategoryProduct.Goods];
    }

    [Fact]
    public async Task CreateProduct_ProductIsValid_ShouldReturnCreatedProductId()
    {
        // Arrange
        const int expectedProductId = 1;
        AutoFaker.Configure(f => f.WithConventions());
        var correctCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.CreateProductAsync(correctCreateProductRequest);

        // Assert
        response.Id.Should().Be(expectedProductId);
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidCategory_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectCategoryCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, Goods.Types.CategoryGoods.None)
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.CreateProductAsync(incorrectCategoryCreateProductRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidWarehouseId_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectWarehouseIdCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(int.MinValue, 0))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.CreateProductAsync(incorrectWarehouseIdCreateProductRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidPrice_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectPriceCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(double.MinValue, 0))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.CreateProductAsync(incorrectPriceCreateProductRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidWeight_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectWeightCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(double.MinValue, 0))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.CreateProductAsync(incorrectWeightCreateProductRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNameMinLength_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const string minLengthName = "";
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectNameMinLengthCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, minLengthName)
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.CreateProductAsync(incorrectNameMinLengthCreateProductRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNameMaxLength_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        var maxLengthName = new string(Enumerable.Repeat('a', 129).ToArray());
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectNameMaxLengthCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, maxLengthName)
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.CreateProductAsync(incorrectNameMaxLengthCreateProductRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsValid_ShouldReturnOk()
    {
        // Arrange
        AutoFaker.Configure(f => f.WithConventions());
        var correctUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();

        // Act & Assert
        var client = new ProductStorage.ProductStorageClient(Channel);
        await client.UpdatePriceProductAsync(correctUpdatePriceRequest);
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsNotValidId_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectIdUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(int.MinValue, 0))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.UpdatePriceProductAsync(incorrectIdUpdatePriceRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsNotValidPrice_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectPriceUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(double.MinValue, 0))
            .Generate();

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.UpdatePriceProductAsync(incorrectPriceUpdatePriceRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task GetProductById_IsValidProductId_ShouldReturnProduct()
    {
        // Arrange
        var expectedProduct = _correctListProduct.First();
        AutoFaker.Configure(f => f.WithConventions());
        var validProductIdGetProductByIdRequest = new GetProductByIdRequest
        {
            Id = expectedProduct.Id
        };

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetProductByIdAsync(validProductIdGetProductByIdRequest);

        // Assert
        response.Product.Should()
            .BeEquivalentTo(expectedProduct, option => option
                .Excluding(o => o.CreateDate)
                .Excluding(o => o.CategoryProduct));
    }

    [Fact]
    public async Task GetProductById_IsNotValidProductId_ShouldReturnErrorInternal()
    {
        // Arrange
        const int isNotValidId = -1;
        const StatusCode expectedStatusCode = StatusCode.Internal;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectProductIdGetProductByIdRequest = new GetProductByIdRequest
        {
            Id = isNotValidId
        };

        // Act & Assert
        try
        {
            var client = new ProductStorage.ProductStorageClient(Channel);
            await client.GetProductByIdAsync(incorrectProductIdGetProductByIdRequest);
        }
        catch (RpcException e)
        {
            e.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    [Fact]
    public async Task GetListProduct_SetDefaultPaginationAndFilter_ShouldReturnFirstPageProduct()
    {
        // Arrange
        const int expectedAmountProducts = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var defaultGoodsListRequest = new Faker<GoodsListRequest>().Generate();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(defaultGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetIsValidPage_ShouldReturnSelectedNumberPageProduct()
    {
        // Arrange
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var pageGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, page => page.Random.Number(1, 100))
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter())
            .Generate();
        var expectedAmountProducts = _correctListProduct
            .Skip((pageGoodsListRequest.Page - 1) * defaultPageSize)
            .Take(defaultPageSize)
            .Count();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(pageGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetIsValidPageSize_ShouldReturnFirstPageSelectedSize()
    {
        // Arrange
        AutoFaker.Configure(f => f.WithConventions());
        var pageSizeGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, pageSize => pageSize.Random.Number(1, 100))
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter())
            .Generate();
        var expectedAmountProducts = _correctListProduct
            .Take(pageSizeGoodsListRequest.PageSize)
            .Count();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(pageSizeGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Theory]
    [MemberData(nameof(CategoryFilter))]
    public async Task GetListProduct_SetFilterCategory_ShouldReturnProductsCategory(CategoryProduct category)
    {
        // Arrange
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var generalGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
                {
                    Category = (Goods.Types.CategoryGoods)category
                }
            )
            .Generate();
        var expectedAmountProducts = _correctListProduct
            .Where(p => p.CategoryProduct == category)
            .Take(defaultPageSize)
            .Count();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(generalGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterCategoryNone_ShouldReturnProductsAnyCategory()
    {
        // Arrange
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var anyGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
                {
                    Category = Goods.Types.CategoryGoods.None
                }
            )
            .Generate();
        var expectedAmountProducts = _correctListProduct
            .Take(defaultPageSize)
            .Count();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(anyGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterDateToday_ShouldReturnProductsDateCreatedToday()
    {
        // Arrange
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var createdTodayGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
            {
                Date = DateTime.UtcNow.ToTimestamp()
            })
            .Generate();
        var expectedAmountProducts = _correctListProduct
            .Where(p => p.CreateDate == createdTodayGoodsListRequest.Filter.Date.ToDateTimeOffset())
            .Take(defaultPageSize)
            .Count();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(createdTodayGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterAnyWarehouseId_ShouldReturnProductsAnyWarehouseId()
    {
        // Arrange
        const int defaultPageSize = 10;
        const int anyWarehouseId = 0;
        AutoFaker.Configure(f => f.WithConventions());
        var anyWarehouseGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
            {
                Warehouse = anyWarehouseId
            })
            .Generate();
        var expectedAmountProducts = _correctListProduct
            .Take(defaultPageSize)
            .Count();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(anyWarehouseGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterWarehouseId_ShouldReturnProductsAnyWarehouseId()
    {
        // Arrange
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var selectedWarehouseGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, warehouse => new GoodsListRequest.Types.Filter
            {
                Warehouse = warehouse.Random.Number(1, 1000)
            })
            .Generate();
        var expectedAmountProducts = _correctListProduct
            .Where(p => p.WarehouseId == selectedWarehouseGoodsListRequest.Filter.Warehouse)
            .Take(defaultPageSize)
            .Count();

        // Act
        var client = new ProductStorage.ProductStorageClient(Channel);
        var response = await client.GetListProductAsync(selectedWarehouseGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
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