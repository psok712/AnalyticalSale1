using ProductService.Domain.Dto;
using ProductService.Domain.Models;

namespace ProductService.Domain.Interfaces;

public interface IProductService
{
    long CreateProduct(ProductDto productDto);
    Product GetProductById(long productId);
    void UpdatePriceProduct(long productId, double price);
    IEnumerable<Product> GetListProducts(GetListProductDto getListProductDto);
}