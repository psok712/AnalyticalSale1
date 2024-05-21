namespace ProductService.Domain.Models;

public class Product
{
    public long Id { get; set; }
    public required string Name { get; init; }
    public double Price { get; set; }
    public double Weight { get; init; }
    public CategoryProduct CategoryProduct { get; init; }
    public DateTimeOffset CreateDate { get; init; }
    public long WarehouseId { get; init; }
}