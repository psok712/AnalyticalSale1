using AutoBogus;
using AutoBogus.Conventions;
using Bogus;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Ozon.ProductService.Api;
using Ozon.ProductService.DataAccess.Exceptions;
using Ozon.ProductService.Domain.Interfaces;
using Ozon.ProductService.Domain.Models;
using Ozon.ProductService.IntegrationTest.GrpcHelpers;
using Xunit.Abstractions;
using Enum = System.Enum;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Ozon.ProductService.IntegrationTest;

public class ProductServiceTestsGrpc : IntegrationTestBase
{
    private readonly Mock<IProductRepository> _productRepositoryFake = new(MockBehavior.Strict);

    public ProductServiceTestsGrpc(
        GrpcTestFixture<Startup> fixture,
        ITestOutputHelper outputHelper
    ) : base(fixture, outputHelper)
    {
        const int expectedCreateProductId = 1;
        const int incorrectProductId = -1;
        var correctListProduct = GenerateValidListProducts(20);
        const int getProductId = 1;

        _productRepositoryFake
            .Setup(f => f.Add(It.IsAny<Product>()))
            .Returns(expectedCreateProductId);

        _productRepositoryFake
            .Setup(f => f.UpdatePrice(It.IsAny<long>(), It.IsAny<double>()));

        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(correctListProduct);
        
        _productRepositoryFake
            .Setup(f => f.Get(getProductId))
            .Returns(correctListProduct.Where(p => p.Id == getProductId).First);
        _productRepositoryFake
            .Setup(f => f.Get(incorrectProductId))
            .Throws(new NotFoundProductException("No product found with this ID."));

        fixture.ConfigureWebHost(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(new ServiceDescriptor(typeof(IProductRepository), _productRepositoryFake.Object));
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
        var client = new ProductStorage.ProductStorageClient(Channel);
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
        var response = await client.CreateProductAsync(correctCreateProductRequest);

        // Assert
        response.Id.Should().Be(expectedProductId);
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidCategory_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        var client = new ProductStorage.ProductStorageClient(Channel);
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectCategoryCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, Goods.Types.CategoryGoods.None)
            .Generate();

        // Act
        try
        {
            await client.CreateProductAsync(incorrectCategoryCreateProductRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidWarehouseId_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        var client = new ProductStorage.ProductStorageClient(Channel);
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

        // Act
        try
        {
            await client.CreateProductAsync(incorrectWarehouseIdCreateProductRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidPrice_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        var client = new ProductStorage.ProductStorageClient(Channel);
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

        // Act
        try
        {
            await client.CreateProductAsync(incorrectPriceCreateProductRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidWeight_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        var client = new ProductStorage.ProductStorageClient(Channel);
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

        // Act
        try
        {
            await client.CreateProductAsync(incorrectWeightCreateProductRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNameMinLength_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const string minLengthName = "";
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        var client = new ProductStorage.ProductStorageClient(Channel);
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

        // Act
        try
        {
            await client.CreateProductAsync(incorrectNameMinLengthCreateProductRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task CreateProduct_ProductIsNameMaxLength_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        var maxLengthName = new string(Enumerable.Repeat('a', 129).ToArray());
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        var client = new ProductStorage.ProductStorageClient(Channel);
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

        // Act
        try
        {
            await client.CreateProductAsync(incorrectNameMaxLengthCreateProductRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsValid_ShouldReturnOk()
    {
        // Arrange
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var correctUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();

        // Act
        await client.UpdatePriceProductAsync(correctUpdatePriceRequest);
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsNotValidId_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectIdUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(int.MinValue, 0))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();

        // Act
        try
        {
            await client.UpdatePriceProductAsync(incorrectIdUpdatePriceRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsNotValidPrice_ShouldReturnErrorInvalidArgument()
    {
        // Arrange
        const StatusCode expectedStatusCode = StatusCode.InvalidArgument;
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectPriceUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(double.MinValue, 0))
            .Generate();

        // Act
        try
        {
            await client.UpdatePriceProductAsync(incorrectPriceUpdatePriceRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task GetProductById_IsValidProductId_ShouldReturnProduct()
    {
        // Arrange
        const int expectedProductId = 1;
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var validProductIdGetProductByIdRequest = new GetProductByIdRequest
        {
            Id = expectedProductId
        };

        // Act
        var response = await client.GetProductByIdAsync(validProductIdGetProductByIdRequest);

        // Assert
        response.Product.Id.Should().Be(expectedProductId);
    }

    [Fact]
    public async Task GetProductById_IsNotValidProductId_ShouldReturnErrorInternal()
    {
        // Arrange
        const int isNotValidId = -1;
        var client = new ProductStorage.ProductStorageClient(Channel);
        const StatusCode expectedStatusCode = StatusCode.Internal;
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectProductIdGetProductByIdRequest = new GetProductByIdRequest
        {
            Id = isNotValidId
        };

        // Act
        try
        {
            await client.GetProductByIdAsync(incorrectProductIdGetProductByIdRequest);
        }
        catch (RpcException e) when (e.StatusCode == expectedStatusCode)
        {
            // Assert
            e.StatusCode.Should().Be(expectedStatusCode);
        }
        catch (RpcException)
        {
            Assert.Fail("RpcException with expected StatusCode was not thrown.");
        }
    }

    [Fact]
    public async Task GetListProduct_SetDefaultPaginationAndFilter_ShouldReturnFirstPageProduct()
    {
        // Arrange
        const int expectedAmountProducts = 10;
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var defaultGoodsListRequest = new Faker<GoodsListRequest>().Generate();

        // Act
        var response = await client.GetListProductAsync(defaultGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetIsValidPage_ShouldReturnSelectedNumberPageProduct()
    {
        // Arrange
        const int defaultPageSize = 10;
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var pageGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, page => page.Random.Number(1, 100))
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter())
            .Generate();
        var expectedAmountProducts = _productRepositoryFake.Object.List()
            .Skip((pageGoodsListRequest.Page - 1) * defaultPageSize)
            .Take(defaultPageSize)
            .Count();

        // Act
        var response = await client.GetListProductAsync(pageGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetIsValidPageSize_ShouldReturnFirstPageSelectedSize()
    {
        // Arrange
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var pageSizeGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, pageSize => pageSize.Random.Number(1, 100))
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter())
            .Generate();
        var expectedAmountProducts = _productRepositoryFake.Object.List()
            .Take(pageSizeGoodsListRequest.PageSize)
            .Count();

        // Act
        var response = await client.GetListProductAsync(pageSizeGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Theory]
    [MemberData(nameof(CategoryFilter))]
    public async Task GetListProduct_SetFilterCategory_ShouldReturnProductsCategory(CategoryProduct category)
    {
        // Arrange
        var client = new ProductStorage.ProductStorageClient(Channel);
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

        // Act
        var response = await client.GetListProductAsync(generalGoodsListRequest);

        // Assert
        response.Product.All(product => product.Category == (Goods.Types.CategoryGoods)category).Should().BeTrue();
    }

    [Fact]
    public async Task GetListProduct_SetFilterCategoryNone_ShouldReturnProductsAnyCategory()
    {
        // Arrange
        const int defaultPageSize = 10;
        var client = new ProductStorage.ProductStorageClient(Channel);
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
        var expectedAmountProducts = _productRepositoryFake.Object.List()
            .Take(defaultPageSize)
            .Count();

        // Act
        var response = await client.GetListProductAsync(anyGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterDateToday_ShouldReturnProductsDateCreatedToday()
    {
        // Arrange
        const int defaultPageSize = 10;
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var createdTodayGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
            {
                Date = DateTime.UtcNow.ToTimestamp()
            })
            .Generate();
        var expectedAmountProducts = _productRepositoryFake.Object.List()
            .Where(p => p.CreateDate == createdTodayGoodsListRequest.Filter.Date.ToDateTimeOffset())
            .Take(defaultPageSize)
            .Count();

        // Act
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
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var anyWarehouseGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
            {
                Warehouse = anyWarehouseId
            })
            .Generate();
        var expectedAmountProducts = _productRepositoryFake.Object.List()
            .Take(defaultPageSize)
            .Count();

        // Act
        var response = await client.GetListProductAsync(anyWarehouseGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterWarehouseId_ShouldReturnProductsAnyWarehouseId()
    {
        // Arrange
        const int defaultPageSize = 10;
        var client = new ProductStorage.ProductStorageClient(Channel);
        AutoFaker.Configure(f => f.WithConventions());
        var selectedWarehouseGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, warehouse => new GoodsListRequest.Types.Filter
            {
                Warehouse = warehouse.Random.Number(1, 1000)
            })
            .Generate();
        var expectedAmountProducts = _productRepositoryFake.Object.List()
            .Where(p => p.WarehouseId == selectedWarehouseGoodsListRequest.Filter.Warehouse)
            .Take(defaultPageSize)
            .Count();

        // Act
        var response = await client.GetListProductAsync(selectedWarehouseGoodsListRequest);

        // Assert
        response.Product.Should().HaveCount(expectedAmountProducts);
    }
    
    private static List<Product> GenerateValidListProducts(int count)
    {
        var listProduct =  new AutoFaker<Product>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, faker
                => (CategoryProduct)faker.Random.Number(1, Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate(count - 1);
        listProduct.Add(new AutoFaker<Product>().RuleFor(f => f.Id, 1).Generate());
        return listProduct;
    }
}