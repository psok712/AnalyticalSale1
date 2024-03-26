using System.Collections.Concurrent;
using Ozon.ProductService.DataAccess.Exceptions;
using Ozon.ProductService.Domain.Interfaces;
using Ozon.ProductService.Domain.Models;

namespace Ozon.ProductService.DataAccess;

public class ProductRepository : IProductRepository
{
    private readonly object _dictionaryLock = new();
    private readonly ConcurrentDictionary<long, Product> _store = new();
    private long _currentId = 1;

    public long Add(Product product)
    {
        product.Id = _currentId;
        lock (_dictionaryLock)
        {
            if (_store.TryAdd(_currentId, product))
                return _currentId++;
        }

        throw new ProductExistException(
            "It was not possible to create a product with the same ID already exists.");
    }

    public Product Get(long id)
    {
        return _store.TryGetValue(id, out var value)
            ? value
            : throw new NotFoundProductException("No product found with this ID.");
    }

    public void UpdatePrice(long id, double price)
    {
        lock (_dictionaryLock)
        {
            if (_store.TryRemove(id, out var product))
            {
                product.Price = price;
                _store.TryAdd(product.Id, product);
                return;
            }
        }

        throw new NotFoundProductException("No product found with this ID.");
    }

    public IEnumerable<Product> List()
    {
        return _store.Values;
    }
}