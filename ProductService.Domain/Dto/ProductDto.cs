using ProductService.Domain.Models;

namespace ProductService.Domain.Dto;

public record ProductDto(
    string Name,
    double Price,
    double Weight,
    CategoryProduct CategoryProduct,
    long WarehouseId
);