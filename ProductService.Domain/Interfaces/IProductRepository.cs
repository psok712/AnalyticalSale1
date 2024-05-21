using ProductService.Domain.Models;

namespace ProductService.Domain.Interfaces;

public interface IProductRepository
{
    long Add(Product product);
    Product Get(long id);
    void UpdatePrice(long id, double price);
    IEnumerable<Product> List();
}