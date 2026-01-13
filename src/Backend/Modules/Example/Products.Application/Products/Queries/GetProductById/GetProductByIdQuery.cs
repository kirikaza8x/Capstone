using Shared.Application.Messaging;

namespace Products.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDetailResponse>;

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string? CreatedBy,
    DateTime? CreatedAt,
    string? ModifiedBy,
    DateTime? ModifiedAt);