using Ozon.ProductService.Domain.Dto;
using Ozon.ProductService.Domain.Models;

namespace Ozon.ProductService.Domain.Interfaces;

public interface IProductService
{
    long CreateProduct(ProductDto productDto);
    Product GetProductById(long productId);
    void UpdatePriceProduct(long productId, double price);
    IEnumerable<Product> GetListProducts(GetListProductDto getListProductDto);
}