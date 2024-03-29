using AutoBogus;
using AutoBogus.Conventions;
using FluentAssertions;
using Moq;
using Ozon.ProductService.Domain.Dto;
using Ozon.ProductService.Domain.Interfaces;
using Ozon.ProductService.Domain.Models;

namespace Ozon.ProductService.UniteTest;


public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryFake = new(MockBehavior.Strict);

    private readonly IProductService _productService;
    private readonly List<Product> _listProduct;
    private const int DefaultPageSize = 10;

    public ProductServiceTests()
    {
        _productService = new Service.ProductService(_productRepositoryFake.Object);
        AutoFaker.Configure(f => f.WithConventions());
        _listProduct = new AutoFaker<Product>()
            .RuleFor(f => f.Id, id => id.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.WarehouseId, warehouseId => warehouseId.Random.Number(1, int.MaxValue))
            .RuleFor(f => f.Price, price => price.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Weight, weight => weight.Random.Double(1, double.MaxValue))
            .RuleFor(f => f.Name, name => name.Name.JobArea())
            .RuleFor(f => f.CategoryProduct, faker 
                => (CategoryProduct)faker.Random.Number(1, Enum.GetValues(typeof(CategoryProduct)).Length - 1))
            .Generate(30);
    }
    
    public static IEnumerable<Object[]> CategoryFilter()
    {
        yield return [CategoryProduct.General];
        yield return [CategoryProduct.HouseholdChemicals];
        yield return [CategoryProduct.Technique];
        yield return [CategoryProduct.Goods];
    }

    [Fact]
    public void GetListProducts_WithoutFilteringAndPagination_ShouldReturnFirstPageProducts()
    {
        var defaultListProductDto = new GetListProductDto(0, 0, null, 0, 0);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);


        var actualListProducts = _productService.GetListProducts(defaultListProductDto);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetMaxPageNumber_ShouldReturnLastDefaultPageProducts()
    {
        const int maxPageNumber = int.MaxValue;
        var maxPageListProductFilter = new GetListProductDto(Page: maxPageNumber, 0, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);


        var actualListProducts = _productService.GetListProducts(maxPageListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEmpty();
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetMinPageNumber_ShouldReturnFirstDefaultPageProducts()
    {
        const int minPageNumber = int.MinValue;
        var minPageListProductFilter = new GetListProductDto(Page: minPageNumber, 0, null, 0, 0);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);


        var actualListProducts = _productService.GetListProducts(minPageListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetPageNumberTwo_ShouldReturnSecondPageProducts()
    {
        const int pageNumber = 2;
        var pageNumberTwoListProductFilter = new GetListProductDto(Page: pageNumber, 0, null, 0, 0);
        var expectedListProduct = _listProduct
            .Skip((pageNumber - 1) * DefaultPageSize)
            .Take(DefaultPageSize);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);


        var actualListProducts = _productService.GetListProducts(pageNumberTwoListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetMaxPageSize_ShouldReturnMaxAmountProducts()
    {
        const int maxPageSize = int.MaxValue;
        var maxPageSizeListProductFilter = new GetListProductDto(0, PageSize: maxPageSize, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);


        var actualListProducts = _productService.GetListProducts(maxPageSizeListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(_listProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetMinPageSize_ShouldReturnEmptyListProduct()
    {
        const int minPageSize = int.MinValue;
        var minPageSizeListProductFilter = new GetListProductDto(0, PageSize: minPageSize, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);


        var actualListProducts = _productService.GetListProducts(minPageSizeListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEmpty();
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetPageSizeToFive_ShouldReturnFiveFirstProducts()
    {
        const int pageSize = 5;
        var pageSizeToFiveListProductFilter = new GetListProductDto(0, PageSize: pageSize, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct.Take(pageSize);


        var actualListProducts = _productService.GetListProducts(pageSizeToFiveListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetDateTimeFilters_ShouldReturnCreatedProductsInSpecifiedDate()
    {
        var firstProductDate = _listProduct.First().CreateDate.DateTime;
        var dateTimeListProductFilter = new GetListProductDto(0, 0, DateTime: firstProductDate, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CreateDate == firstProductDate)
            .Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(dateTimeListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Theory]
    [MemberData(nameof(CategoryFilter))]
    public void GetListProducts_SetCategoryProduct_ShouldReturnFirstPageProductsCategory(CategoryProduct category)
    {
        var generalCategoryListProductFilter = new GetListProductDto(0, 0, null, CategoryProduct: category, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CategoryProduct == category)
            .Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(generalCategoryListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetCategoryProductAtNone_ShouldReturnFirstPageProductsAnyCategory()
    {
        const CategoryProduct categoryProduct = CategoryProduct.None;
        var anyCategoryListProductFilter = new GetListProductDto(0, 0, null, CategoryProduct: categoryProduct, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(anyCategoryListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    
    [Fact]
    public void GetListProducts_SetWarehouseIdAtDefault_ShouldReturnFirstPageProductAnyWarehouseId()
    {
        const int anyWarehouseId = 0;
        var minWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, WarehouseId: anyWarehouseId);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(minWarehouseIdListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetMaxWarehouseId_ShouldReturnFirstPageProductWithMaxWarehouseId()
    {
        const int maxWarehouseId = int.MaxValue;
        var maxWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, WarehouseId: maxWarehouseId);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.WarehouseId == int.MaxValue)
            .Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(maxWarehouseIdListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetAnyWarehouseId_ShouldReturnFirstPageProductToWarehouseId()
    {
        var anyWarehouseId = _listProduct.First().WarehouseId;
        var anyPositiveWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, WarehouseId: anyWarehouseId);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.WarehouseId == anyWarehouseId)
            .Take(DefaultPageSize);
        
        
        var actualListProducts = _productService.GetListProducts(anyPositiveWarehouseIdListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
}