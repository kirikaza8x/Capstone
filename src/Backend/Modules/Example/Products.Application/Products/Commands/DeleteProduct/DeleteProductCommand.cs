using Shared.Application.Messaging;

namespace Products.Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand;