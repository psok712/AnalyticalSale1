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
    public async Task CreateProduct_CreateCorrectProduct_ShouldReturnProductIdAndHttpStatusOk()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(correctCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = CreateProductResponse.Parser.ParseJson(responseBody);
        productsPage.Id.Should().Be(expectedProductId);
    }

    [Fact]
    public async Task CreateProduct_CreateProductWithIncorrectCategory_ShouldReturnHttpStatusBadRequest()
    {
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectCategoryCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, Goods.Types.CategoryGoods.None)
            .Generate();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectCategoryCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_CreateProductWithIncorrectWarehouseId_ShouldReturnHttpStatusBadRequest()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectWarehouseIdCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_CreateProductWithIncorrectPrice_ShouldReturnHttpStatusBadRequest()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectPriceCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_CreateProductWithIncorrectWeight_ShouldReturnHttpStatusBadRequest()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectWeightCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_CreateProductWithIncorrectNameMinLength_ShouldReturnHttpStatusBadRequest()
    {
        const string minLengthLine = "";
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectNameMinLengthCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, minLengthLine)
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectNameMinLengthCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_CreateProductWithIncorrectNameMaxLength_ShouldReturnHttpStatusBadRequest()
    {
        var maxLengthLine = new string(Enumerable.Repeat('a', 129).ToArray());
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectNameMaxLengthCreateProductRequest = new AutoFaker<CreateProductRequest>()
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, maxLengthLine)
            .RuleFor(f => f.CategoryProduct, faker
                => (Goods.Types.CategoryGoods)faker.Random.Number(1,
                    Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectNameMaxLengthCreateProductRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/product/create", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePriceProduct_UpdateProductWithCorrectIdAndPrice_ShouldReturnHttpStatusOk()
    {
        AutoFaker.Configure(f => f.WithConventions());
        var correctUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(correctUpdatePriceRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/update", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePriceProduct_UpdateProductWithIncorrectId_ShouldReturnHttpStatusBadRequest()
    {
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectIdUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(int.MinValue, 0))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .Generate();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectIdUpdatePriceRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/update", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePriceProduct_UpdateProductWithIncorrectPrice_ShouldReturnHttpStatusBadRequest()
    {
        AutoFaker.Configure(f => f.WithConventions());
        var incorrectPriceUpdatePriceRequest = new AutoFaker<UpdatePriceRequest>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(double.MinValue, 0))
            .Generate();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(incorrectPriceUpdatePriceRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/update", content);


        response.Should().Match(r => r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_SetCorrectProductId_ShouldReturnProductAndHttpStatusOk()
    {
        var expectedProduct = _webApplicationFactory.CorrectListProduct.First();
        AutoFaker.Configure(f => f.WithConventions());
        var firstCorrectProductIdGetProductByIdRequest = _webApplicationFactory.CorrectListProduct.First().Id;


        var client = _webApplicationFactory.CreateClient();
        var response = await client.GetAsync($"/api/v1/product/{firstCorrectProductIdGetProductByIdRequest}");


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GetProductByIdResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().BeEquivalentTo(expectedProduct,
            option => option.Excluding(o => o.CreateDate).Excluding(o => o.CategoryProduct));
    }

    [Fact]
    public async Task GetProductById_SetIncorrectProductId_ShouldReturnInternalServerError()
    {
        AutoFaker.Configure(f => f.WithConventions());
        const int incorrectProductIdGetProductByIdRequest = -1;


        var client = _webApplicationFactory.CreateClient();
        var response = await client.GetAsync($"/api/v1/product/{incorrectProductIdGetProductByIdRequest}");


        response.Should().Match(r => r.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetListProduct_SetDefaultPaginationAndFilter_ShouldReturnFirstPageAndHttpStatusOk()
    {
        const int expectedAmountProducts = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var defaultGoodsListRequest = new Faker<GoodsListRequest>().Generate();
        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(defaultGoodsListRequest, optionsJson);


        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetCorrectPaginationPage_ShouldReturnSelectedNumberPageAndHttpStatusOk()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(pageGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetCorrectPaginationPageSize_ShouldReturnFirstPageSelectedSizeAndHttpStatusOk()
    {
        AutoFaker.Configure(f => f.WithConventions());
        var pageSizeGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, pageSize => pageSize.Random.Number(1, 100))
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter())
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Take(pageSizeGoodsListRequest.PageSize)
            .Count();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(pageSizeGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterCategoryGeneral_ShouldReturnProductsCategoryAtGeneralAndHttpStatusOk()
    {
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var generalGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
                {
                    Category = Goods.Types.CategoryGoods.General
                }
            )
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Where(p => p.CategoryProduct == CategoryProduct.General)
            .Take(defaultPageSize)
            .Count();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(generalGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterCategoryGoods_ShouldReturnProductsCategoryAtGoodsAndHttpStatusOk()
    {
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var goodsGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
                {
                    Category = Goods.Types.CategoryGoods.Goods
                }
            )
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Where(p => p.CategoryProduct == CategoryProduct.Goods)
            .Take(defaultPageSize)
            .Count();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(goodsGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterCategoryTechnique_ShouldReturnProductsCategoryAtTechniqueAndHttpStatusOk()
    {
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var techniqueGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
                {
                    Category = Goods.Types.CategoryGoods.Technique
                }
            )
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Where(p => p.CategoryProduct == CategoryProduct.Technique)
            .Take(defaultPageSize)
            .Count();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(techniqueGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task
        GetListProduct_SetFilterCategoryHouseholdChemicals_ShouldReturnProductsCategoryAtHouseholdChemicalsAndHttpStatusOk()
    {
        const int defaultPageSize = 10;
        AutoFaker.Configure(f => f.WithConventions());
        var householdChemicalsGoodsListRequest = new AutoFaker<GoodsListRequest>()
            .RuleFor(f => f.Page, 0)
            .RuleFor(f => f.PageSize, 0)
            .RuleFor(f => f.Filter, new GoodsListRequest.Types.Filter
                {
                    Category = Goods.Types.CategoryGoods.HouseholdChemicals
                }
            )
            .Generate();
        var expectedAmountProducts = _webApplicationFactory.CorrectListProduct
            .Where(p => p.CategoryProduct == CategoryProduct.HouseholdChemicals)
            .Take(defaultPageSize)
            .Count();


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(householdChemicalsGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterCategoryNone_ShouldReturnProductsAnyCategoryAndHttpStatusOk()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(anyCategoryGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterDateToday_ShouldReturnProductsDateCreatedTodayAndHttpStatusOk()
    {
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


        var optionsJson = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new TimestampConverter() }
        };
        var productJsonFormat = JsonSerializer.Serialize(createdTodayGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterWarehouseId_ShouldReturnProductsSelectedWarehouseIdAndHttpStatusOk()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(selectedWarehouseGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }

    [Fact]
    public async Task GetListProduct_SetFilterAnyWarehouseId_ShouldReturnProductsAnyWarehouseIdAndHttpStatusOk()
    {
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


        var optionsJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var productJsonFormat = JsonSerializer.Serialize(anyWarehouseGoodsListRequest, optionsJson);

        var client = _webApplicationFactory.CreateClient();
        var content = new StringContent(productJsonFormat, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/store/product", content);


        var responseBody = await response.Content.ReadAsStringAsync();
        response.Should().Match(r => r.StatusCode == HttpStatusCode.OK);

        var productsPage = GoodsListResponse.Parser.ParseJson(responseBody);
        productsPage.Product.Should().HaveCount(expectedAmountProducts);
    }
}