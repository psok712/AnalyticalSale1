using System.Net;
using System.Text;
using System.Text.Json;
using AutoBogus;
using AutoBogus.Conventions;
using Bogus;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc.Testing;
using Ozon.ProductService.Api;
using Ozon.ProductService.Domain.Models;
using DateTime = System.DateTime;
using Enum = System.Enum;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Ozon.ProductService.IntegrationTest;

public class ProductServiceTestsHttp : WebApplicationFactory<CustomDependenciesWebApplicationFactory<Startup>>
{
    private readonly CustomDependenciesWebApplicationFactory<Startup> _webApplicationFactory = new();

    [Fact]
    public async Task CreateProduct_ProductIsValid_ShouldReturnProductIdAndHttpStatusOk()
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
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(correctCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);
        
        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = CreateProductResponse.Parser.ParseJson(responseBody);
        productsPage.Id.Should().Be(expectedProductId);
    }

    public static IEnumerable<object[]> CategoryFilter()
    {
        yield return [CategoryProduct.General];
        yield return [CategoryProduct.HouseholdChemicals];
        yield return [CategoryProduct.Technique];
        yield return [CategoryProduct.Goods];
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidCategory_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectCategoryCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, Goods.Types.CategoryGoods.None)
            .Generate();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectCategoryCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidWarehouseId_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
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
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectWarehouseIdCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidPrice_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
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
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectPriceCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_ProductIsNotValidWeight_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
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
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectWeightCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_ProductIsNameMinLength_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
        const string minLengthName = "";
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
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectNameMinLengthCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_ProductIsNameMaxLength_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
        var maxLengthName = new string(Enumerable.Repeat('a', 129).ToArray());
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
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectNameMaxLengthCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsValid_ShouldReturnHttpStatusOk()
    {
        // Arrange
        AutoFaker.Configure(f => f.WithConventions());
        var correctUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(correctUpdatePriceRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/update", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsNotValidId_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectIdUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(int.MinValue, 0))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectIdUpdatePriceRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/update", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePriceProduct_OptionsUpdateIsNotValidPrice_ShouldReturnHttpStatusBadRequest()
    {
        // Arrange
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectPriceUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(double.MinValue, 0))
            .Generate();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectPriceUpdatePriceRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/update", content);

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_IsValidProductId_ShouldReturnProduct()
    {
        // Arrange
        var expectedProduct = _webApplicationFactory.CorrectListProduct.First();
        AutoFaker.Configure(f => f.WithConventions());
        var validProductIdGetProductByIdRequest = _webApplicationFactory.CorrectListProduct.First().Id;

        // Act
        var client = _webApplicationFactory.CreateClient();
        var response = await client.GetAsync($"/api/v1/product/{validProductIdGetProductByIdRequest}");

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GetProductByIdResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().BeEquivalentTo(expectedProduct,
            option => option
                .Excluding(o => o.CreateDate)
                .Excluding(o => o.CategoryProduct));
    }

    [Fact]
    public async Task GetProductById_IsNotValidProductId_ShouldReturnInternalServerError()
    {
        // Arrange
        const int incorrectProductIdGetProductByIdRequest = -1;

        // Act
        var client = _webApplicationFactory.CreateClient();
        var response = await client.GetAsync($"/api/v1/product/{incorrectProductIdGetProductByIdRequest}");

        // Assert
        response.Should().Match(r => r.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetListProduct_SetDefaultPaginationAndFilter_ShouldReturnFirstPageAndHttpStatusOk()
    {
        // Arrange
        const int expectedAmountProducts = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var defaultGoodsListRequest = new Faker<GoodsListRequest>().Generate();
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(defaultGoodsListRequest, optionsJson);

        // Act
        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetIsValidPage_ShouldReturnSelectedNumberPageAndHttpStatusOk()
    {
        // Arrange
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var pageGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, page => page.Random.Number(1, 100))
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter())
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Skip((pageGoodsListRequest.Page - 1) * defaultPageSize)
            .Take(defaultPageSize)
            .Count();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(pageGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetIsValidPageSize_ShouldReturnFirstPageSelectedSizeAndHttpStatusOk()
    {
        // Arrange
        AutoFaker.Configure(f => f.WithConventions());
        var pageSizeGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, pageSize => pageSize.Random.Number(1, 100))
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter())
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Take(pageSizeGoodsListRequest.PageSize)
            .Count();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(pageSizeGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Theory]
    [MemberData(nameof(CategoryFilter))]
    public async Task GetListProduct_SetFilterCategory_ShouldReturnProductsCategoryAndHttpStatusOk(
        CategoryProduct category)
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
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Where(p => p.CategoryProduct == category)
            .Take(defaultPageSize)
            .Count();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(generalGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterCategoryNone_ShouldReturnProductsAnyCategoryAndHttpStatusOk()
    {
        // Arrange
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var anyCategoryGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
                {
                    Category = Goods.Types.CategoryGoods.None
                }
            )
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Take(defaultPageSize)
            .Count();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(anyCategoryGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterDateToday_ShouldReturnProductsDateCreatedTodayAndHttpStatusOk()
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
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Where(p => p.CreateDate == createdTodayGoodsListRequest.Filter.Date.ToDateTimeOffset())
            .Take(defaultPageSize)
            .Count();

        // Act
        var optionsJson = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new TimestampConverter() }
        };
        var productJsonFormat = JsonSerializer.Serialize(createdTodayGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterWarehouseId_ShouldReturnProductsSelectedWarehouseIdAndHttpStatusOk()
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
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Where(p => p.WarehouseId == selectedWarehouseGoodsListRequest.Filter.Warehouse)
            .Take(defaultPageSize)
            .Count();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(selectedWarehouseGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterAnyWarehouseId_ShouldReturnProductsAnyWarehouseIdAndHttpStatusOk()
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
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Take(defaultPageSize)
            .Count();

        // Act
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(anyWarehouseGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }
}