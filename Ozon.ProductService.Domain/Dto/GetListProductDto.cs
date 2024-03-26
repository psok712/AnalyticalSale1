using Ozon.ProductService.Domain.Models;

namespace Ozon.ProductService.Domain.Dto;

public record GetListProductDto(
    int Page,
    int PageSize,
    DateTime? DateTime,
    CategoryProduct? CategoryProduct,
    long? WarehouseId
);