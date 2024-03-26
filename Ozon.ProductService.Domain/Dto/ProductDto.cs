using Ozon.ProductService.Domain.Models;

namespace Ozon.ProductService.Domain.Dto;

public record ProductDto(
    string Name,
    double Price,
    double Weight,
    CategoryProduct CategoryProduct,
    long WarehouseId
);