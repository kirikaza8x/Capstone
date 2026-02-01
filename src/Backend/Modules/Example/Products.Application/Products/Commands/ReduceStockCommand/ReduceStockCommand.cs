
using Shared.Application.Messaging;

namespace Products.Application.Products.Commands.ReduceStockCommand;

public sealed record ReduceStockCommand(List<ReduceStockItem> Items) : ICommand;

public sealed record ReduceStockItem(Guid ProductId, int Quantity);