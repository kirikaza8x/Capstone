using Shared.Application.Messaging;

namespace Products.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock) : ICommand<Guid>;