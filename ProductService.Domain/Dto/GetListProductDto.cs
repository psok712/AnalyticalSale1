using ProductService.Domain.Models;

namespace ProductService.Domain.Dto;

public record GetListProductDto(
    int Page,
    int PageSize,
    DateTime? DateTime,
    CategoryProduct? CategoryProduct,
    long? WarehouseId
);