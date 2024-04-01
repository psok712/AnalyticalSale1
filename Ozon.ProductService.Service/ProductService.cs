using Ozon.ProductService.Domain.Dto;
using Ozon.ProductService.Domain.Interfaces;
using Ozon.ProductService.Domain.Models;

namespace Ozon.ProductService.Service;

public class ProductService(IProductRepository productRepository) : IProductService
{
    public long CreateProduct(ProductDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Price = productDto.Price,
            Weight = productDto.Weight,
            CategoryProduct = productDto.CategoryProduct,
            CreateDate = DateTime.Today,
            WarehouseId = productDto.WarehouseId
        };

        return productRepository.Add(product);
    }

    public Product GetProductById(long productId)
    {
        return productRepository.Get(productId);
    }

    public void UpdatePriceProduct(long productId, double price)
    {
        productRepository.UpdatePrice(productId, price);
    }

    public IEnumerable<Product> GetListProducts(GetListProductDto getListProductDto)
    {
        var pageSize = getListProductDto.PageSize == 0 ? 10 : getListProductDto.PageSize;
        var amountProductSkip = getListProductDto.Page <= 0
            ? 0
            : (getListProductDto.Page - 1) * pageSize >= 0
                ? (getListProductDto.Page - 1) * pageSize
                : int.MaxValue;

        return productRepository
            .List()
            .Where(p =>
                WarehouseFilter(p)
                && CategoryFilter(p)
                && DateFilter(p))
            .Skip(amountProductSkip)
            .Take(pageSize)
            .ToList();

        bool WarehouseFilter(Product p)
        {
            return getListProductDto.WarehouseId == 0 || getListProductDto.WarehouseId == p.WarehouseId;
        }

        bool CategoryFilter(Product p)
        {
            return getListProductDto.CategoryProduct == CategoryProduct.None ||
                   getListProductDto.CategoryProduct == p.CategoryProduct;
        }

        bool DateFilter(Product p)
        {
            return getListProductDto.DateTime == null || getListProductDto.DateTime == p.CreateDate;
        }
    }
}