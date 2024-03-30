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
    private const int DefaultPageSize = 10;
    private readonly List<Product> _listProduct = GenerateValidListProducts(30);
    private readonly Mock<IProductRepository> _productRepositoryFake = new(MockBehavior.Strict);

    private readonly IProductService _productService;

    public ProductServiceTests()
    {
        _productService = new Service.ProductService(_productRepositoryFake.Object);
        AutoFaker.Configure(f => f.WithConventions());
    }

    public static IEnumerable<object[]> CategoryFilter()
    {
        yield return [CategoryProduct.General];
        yield return [CategoryProduct.HouseholdChemicals];
        yield return [CategoryProduct.Technique];
        yield return [CategoryProduct.Goods];
    }

    [Fact]
    public void GetListProducts_WithoutFilteringAndPagination_ShouldReturnFirstPageProducts()
    {
        // Arrange
        var defaultListProductDto = new GetListProductDto(0, 0, null, 0, 0);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        
        // Act
        var actualListProducts = _productService.GetListProducts(defaultListProductDto);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetMaxPageNumber_ShouldReturnLastDefaultPageProducts()
    {
        // Arrange
        const int maxPageNumber = int.MaxValue;
        var maxPageListProductFilter = new GetListProductDto(maxPageNumber, 0, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);

        // Act
        var actualListProducts = _productService.GetListProducts(maxPageListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEmpty();
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetMinPageNumber_ShouldReturnFirstDefaultPageProducts()
    {
        // Arrange
        const int minPageNumber = int.MinValue;
        var minPageListProductFilter = new GetListProductDto(minPageNumber, 0, null, 0, 0);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);

        // Act
        var actualListProducts = _productService.GetListProducts(minPageListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetPageNumberTwo_ShouldReturnSecondPageProducts()
    {
        // Arrange
        const int pageNumber = 2;
        var pageNumberTwoListProductFilter = new GetListProductDto(pageNumber, 0, null, 0, 0);
        var expectedListProduct = _listProduct
            .Skip((pageNumber - 1) * DefaultPageSize)
            .Take(DefaultPageSize);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);

        // Act
        var actualListProducts = _productService.GetListProducts(pageNumberTwoListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetMaxPageSize_ShouldReturnMaxAmountProducts()
    {
        // Arrange
        const int maxPageSize = int.MaxValue;
        var maxPageSizeListProductFilter = new GetListProductDto(0, maxPageSize, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);

        // Act
        var actualListProducts = _productService.GetListProducts(maxPageSizeListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(_listProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetMinPageSize_ShouldReturnEmptyListProduct()
    {
        // Arrange
        const int minPageSize = int.MinValue;
        var minPageSizeListProductFilter = new GetListProductDto(0, minPageSize, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);

        // Act
        var actualListProducts = _productService.GetListProducts(minPageSizeListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEmpty();
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetPageSizeToFive_ShouldReturnFiveFirstProducts()
    {
        // Arrange
        const int pageSize = 5;
        var pageSizeToFiveListProductFilter = new GetListProductDto(0, pageSize, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct.Take(pageSize);

        // Act
        var actualListProducts = _productService.GetListProducts(pageSizeToFiveListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetDateTimeFilters_ShouldReturnCreatedProductsInSpecifiedDate()
    {
        // Arrange
        var firstProductDate = _listProduct.First().CreateDate.DateTime;
        var dateTimeListProductFilter = new GetListProductDto(0, 0, firstProductDate, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CreateDate == firstProductDate)
            .Take(DefaultPageSize);

        // Act
        var actualListProducts = _productService.GetListProducts(dateTimeListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Theory]
    [MemberData(nameof(CategoryFilter))]
    public void GetListProducts_SetCategoryProduct_ShouldReturnFirstPageProductsCategory(CategoryProduct category)
    {
        // Arrange
        var generalCategoryListProductFilter = new GetListProductDto(0, 0, null, category, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CategoryProduct == category)
            .Take(DefaultPageSize);

        // Act
        var actualListProducts = _productService.GetListProducts(generalCategoryListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetCategoryProductAtNone_ShouldReturnFirstPageProductsAnyCategory()
    {
        // Arrange
        const CategoryProduct categoryProduct = CategoryProduct.None;
        var anyCategoryListProductFilter = new GetListProductDto(0, 0, null, categoryProduct, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);

        // Act
        var actualListProducts = _productService.GetListProducts(anyCategoryListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }


    [Fact]
    public void GetListProducts_SetWarehouseIdAtDefault_ShouldReturnFirstPageProductAnyWarehouseId()
    {
        // Arrange
        const int anyWarehouseId = 0;
        var minWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, anyWarehouseId);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);

        // Act
        var actualListProducts = _productService.GetListProducts(minWarehouseIdListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetMaxWarehouseId_ShouldReturnFirstPageProductWithMaxWarehouseId()
    {
        // Arrange
        const int maxWarehouseId = int.MaxValue;
        var maxWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, maxWarehouseId);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.WarehouseId == int.MaxValue)
            .Take(DefaultPageSize);

        // Act
        var actualListProducts = _productService.GetListProducts(maxWarehouseIdListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }

    [Fact]
    public void GetListProducts_SetAnyWarehouseId_ShouldReturnFirstPageProductToWarehouseId()
    {
        // Arrange
        var anyWarehouseId = _listProduct.First().WarehouseId;
        var anyPositiveWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, anyWarehouseId);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.WarehouseId == anyWarehouseId)
            .Take(DefaultPageSize);

        // Act
        var actualListProducts = _productService.GetListProducts(anyPositiveWarehouseIdListProductFilter);

        // Assert
        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
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