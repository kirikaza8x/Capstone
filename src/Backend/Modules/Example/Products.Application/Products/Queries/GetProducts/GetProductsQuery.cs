using Shared.Application.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Products.Application.Products.Query.GetProducts;

public sealed record GetProductsQuery : PagedQuery, IQuery<PagedResult<ProductResponse>>;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime? CreatedAt);