using Shared.Application.Messaging;

namespace Products.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock) : ICommand;