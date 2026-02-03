using Order.Domain.Orders;
using Products.PublicApi;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Order.Application.Orders.Commands.AddOrderItem;

internal sealed class AddOrderItemCommandHandler : ICommandHandler<AddOrderItemCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderUnitOfWork _unitOfWork;
    private readonly IProductsApi _productsApi;

    public AddOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IOrderUnitOfWork unitOfWork,
        IProductsApi productsApi)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _productsApi = productsApi;
    }

    public async Task<Result> Handle(AddOrderItemCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(command.OrderId, cancellationToken);

        Console.WriteLine($"[1] Order loaded: {order?.Id}, Items count: {order?.OrderItems.Count}");

        // ✅ DEBUG: In ra tất cả ProductId của items hiện có
        foreach (var item in order.OrderItems)
        {
            Console.WriteLine($"   - Existing item: ProductId={item.ProductId}, Quantity={item.Quantity}");
        }

        if (order is null)
            return Result.Failure(OrderErrors.NotFound(command.OrderId));

        if (order.Status != OrderStatus.Pending)
            return Result.Failure(OrderErrors.InvalidStatus(order.Status, "add items"));

        var product = await _productsApi.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
            return Result.Failure(OrderErrors.ProductNotFound(command.ProductId));

        if (!product.IsActive)
            return Result.Failure(OrderErrors.ProductNotFound(command.ProductId));

        var inStock = await _productsApi.IsInStockAsync(command.ProductId, command.Quantity, cancellationToken);
        if (!inStock)
            return Result.Failure(OrderErrors.InsufficientStock(command.ProductId));

        Console.WriteLine($"[2] Adding item: ProductId={command.ProductId}, Quantity={command.Quantity}");

        order.AddItem(command.ProductId, product.Name, product.Price, command.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
