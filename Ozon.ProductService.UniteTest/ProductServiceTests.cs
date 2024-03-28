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
        _listProduct = new List<Product>
        {
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate(),
            new AutoFaker<Product>().Generate()
        };
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
    public void GetListProducts_SetMaxPageNumber_ShouldReturnEmptyPageProducts()
    {
        var maxPageListProductFilter = new GetListProductDto(Page: int.MaxValue, 0, null, 0, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);


        var actualListProducts = _productService.GetListProducts(maxPageListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEmpty();
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetMinPageNumber_ShouldReturnFirstPageProducts()
    {
        var minPageListProductFilter = new GetListProductDto(int.MinValue, 0, null, 0, 0);
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
        var pageNumberTwoListProductFilter = new GetListProductDto(pageNumber, 0, null, 0, 0);
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
    public void GetListProducts_SetMaxPageSize_ShouldReturnAllProducts()
    {
        var maxPageSizeListProductFilter = new GetListProductDto(0, PageSize: int.MaxValue, null, 0, 0);
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
        var minPageSizeListProductFilter = new GetListProductDto(0, PageSize: int.MinValue, null, 0, 0);
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
    public void GetListProducts_SetDateTimeFilters_ShouldReturnCreateToday()
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
    
    [Fact]
    public void GetListProducts_SetCategoryProductGeneral_ShouldReturnFirstPageProductGeneral()
    {
        var categoryProduct = CategoryProduct.General;
        var generalCategoryListProductFilter = new GetListProductDto(0, 0, null, CategoryProduct: categoryProduct, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CategoryProduct == categoryProduct)
            .Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(generalCategoryListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetCategoryProductHouseholdChemicals_ShouldReturnFirstPageProductHouseholdChemicals()
    {
        var categoryProduct = CategoryProduct.HouseholdChemicals;
        var householdChemicalsCategoryListProductFilter = new GetListProductDto(0, 0, null, CategoryProduct: categoryProduct, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CategoryProduct == categoryProduct)
            .Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(householdChemicalsCategoryListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetCategoryProductTechnique_ShouldReturnFirstPageProductTechnique()
    {
        const CategoryProduct categoryProduct = CategoryProduct.Technique;
        var techniqueCategoryListProductFilter = new GetListProductDto(0, 0, null, CategoryProduct: categoryProduct, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CategoryProduct == categoryProduct)
            .Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(techniqueCategoryListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetCategoryProductGoods_ShouldReturnFirstPageProductGoods()
    {
        const CategoryProduct categoryProduct = CategoryProduct.Goods;
        var goodsCategoryListProductFilter = new GetListProductDto(0, 0, null, CategoryProduct: categoryProduct, 0);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.CategoryProduct == categoryProduct)
            .Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(goodsCategoryListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetCategoryProductNone_ShouldReturnFirstPageProductAnyCategory()
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
    public void GetListProducts_SetMinWarehouseId_ShouldReturnFirstPageProductAnyWarehouseId()
    {
        var minWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, WarehouseId: int.MinValue);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct.Take(DefaultPageSize);
        

        var actualListProducts = _productService.GetListProducts(minWarehouseIdListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
    
    [Fact]
    public void GetListProducts_SetMaxWarehouseId_ShouldReturnFirstPageProductMaxWarehouseId()
    {
        var maxWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, WarehouseId: int.MaxValue);
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
    public void GetListProducts_SetAnyPositiveWarehouseId_ShouldReturnFirstPageProductToWarehouseId()
    {
        var firstPositiveWarehouseId = _listProduct.First(p => p.WarehouseId > 0).WarehouseId;
        var anyPositiveWarehouseIdListProductFilter = new GetListProductDto(0, 0, null, 0, WarehouseId: firstPositiveWarehouseId);
        _productRepositoryFake
            .Setup(f => f.List())
            .Returns(_listProduct);
        var expectedListProduct = _listProduct
            .Where(p => p.WarehouseId == firstPositiveWarehouseId)
            .Take(DefaultPageSize);
        
        
        var actualListProducts = _productService.GetListProducts(anyPositiveWarehouseIdListProductFilter);

        actualListProducts.Should().NotBeNull().And.BeEquivalentTo(expectedListProduct);
        _productRepositoryFake.Verify(f => f.List(), Times.Once);
    }
}