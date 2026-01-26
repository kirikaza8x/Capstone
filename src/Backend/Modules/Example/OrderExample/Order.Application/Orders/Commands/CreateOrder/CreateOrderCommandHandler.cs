using FluentValidation;
using Order.Domain.Orders;
using Products.PublicApi;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}

internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductsApi _productsApi;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductsApi productsApi)
    {
        _orderRepository = orderRepository;
        _productsApi = productsApi;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // Validate products and stock
        var productIds = command.Items.Select(x => x.ProductId).ToList();
        //
        // Use public API to get product details
        //
        var products = await _productsApi.GetByIdsAsync(productIds, cancellationToken);

        foreach (var item in command.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);

            if (product is null)
                return Result.Failure<Guid>(OrderErrors.ProductNotFound(item.ProductId));

            if (!product.IsActive)
                return Result.Failure<Guid>(OrderErrors.ProductNotFound(item.ProductId));

            var inStock = await _productsApi.IsInStockAsync(item.ProductId, item.Quantity, cancellationToken);
            if (!inStock)
                return Result.Failure<Guid>(OrderErrors.InsufficientStock(item.ProductId));
        }

        var order = Domain.Orders.Order.Create(
            command.CustomerId,
            command.CustomerName,
            command.ShippingAddress);

        foreach (var item in command.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            order.AddItem(item.ProductId, product.Name, product.Price, item.Quantity);
        }

        _orderRepository.Add(order);

        return Result.Success(order.Id);
    }
}