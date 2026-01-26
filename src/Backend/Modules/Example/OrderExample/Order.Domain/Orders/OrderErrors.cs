

using Shared.Domain.Abstractions;

namespace Order.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Order.NotFound",
        $"Order with ID '{id}' was not found");

    public static Error ProductNotFound(Guid productId) => Error.NotFound(
        "Order.ProductNotFound",
        $"Product with ID '{productId}' was not found");

    public static Error InsufficientStock(Guid productId) => Error.Validation(
        "Order.InsufficientStock",
        $"Insufficient stock for product '{productId}'");

    public static Error EmptyOrder => Error.Validation(
        "Order.Empty",
        "Order must have at least one item");

    public static Error InvalidStatus(OrderStatus current, string operation) => Error.Validation(
        "Order.InvalidStatus",
        $"Cannot {operation} order with status '{current}'");
}
