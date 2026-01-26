using Order.Domain.Orders;
using Products.PublicApi;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Order.Application.Orders.Commands.ConfirmOrder;

internal sealed class ConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductsApi _productsApi;

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductsApi productsApi)
    {
        _orderRepository = orderRepository;
        _productsApi = productsApi;
    }

    public async Task<Result> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(command.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure(OrderErrors.NotFound(command.OrderId));

        // Reduce stock for each item
        foreach (var item in order.OrderItems)
        {
            var success = await _productsApi.ReduceStockAsync(
                item.ProductId,
                item.Quantity,
                cancellationToken);

            if (!success)
                return Result.Failure(OrderErrors.InsufficientStock(item.ProductId));
        }

        order.Confirm();
        _orderRepository.Update(order);

        return Result.Success();
    }
}
