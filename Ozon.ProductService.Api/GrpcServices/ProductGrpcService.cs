using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ozon.ProductService.Domain.Dto;
using Ozon.ProductService.Domain.Interfaces;
using Ozon.ProductService.Domain.Models;

namespace Ozon.ProductService.Api.GrpcServices;

public class ProductGrpcService(IProductService productService) : ProductStorage.ProductStorageBase
{
    public override Task<CreateProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        var dto = new ProductDto(
            request.Name,
            request.Price,
            request.Weight,
            (CategoryProduct)request.CategoryProduct,
            request.WarehouseId);
        var productId = productService.CreateProduct(dto);

        return Task.FromResult(new CreateProductResponse
        {
            Id = productId
        });
    }

    public override Task<GetProductByIdResponse> GetProductById(
        GetProductByIdRequest request,
        ServerCallContext context)
    {
        var product = productService.GetProductById(request.Id);

        return Task.FromResult(new GetProductByIdResponse
        {
            Product = CreateGoods(product)
        });
    }

    public override Task<Empty> UpdatePriceProduct(
        UpdatePriceRequest request,
        ServerCallContext context)
    {
        productService.UpdatePriceProduct(request.Id, request.Price);
        return Task.FromResult(new Empty());
    }

    public override Task<GoodsListResponse> GetListProduct(
        GoodsListRequest request,
        ServerCallContext context)
    {
        var dto = CreateGetListProductDto(request);
        var listProducts = productService.GetListProducts(dto);

        return Task.FromResult(
            new GoodsListResponse
            {
                Product = { listProducts.Select(CreateGoods) }
            }
        );
    }

    private static GetListProductDto CreateGetListProductDto(GoodsListRequest request)
    {
        return request.Filter == null
            ? new GetListProductDto(request.Page, request.PageSize, null, 0, 0)
            : new GetListProductDto(
                request.Page,
                request.PageSize,
                request.Filter.Date?.ToDateTime(),
                (CategoryProduct)request.Filter.Category,
                request.Filter.Warehouse);
    }

    private static Goods CreateGoods(Product p)
    {
        return new Goods
        {
            Name = p.Name,
            WarehouseId = p.WarehouseId,
            Category = (Goods.Types.CategoryGoods)p.CategoryProduct,
            CreateDate = p.CreateDate.ToTimestamp(),
            Id = p.Id,
            Price = p.Price,
            Weight = p.Weight
        };
    }
}