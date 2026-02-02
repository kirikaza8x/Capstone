using Order.Domain.Orders.Events;
using Shared.Domain.DDD;

namespace Order.Domain.Orders;

public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount => _orderItems.Sum(x => x.TotalPrice);

    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order() { }

    private Order(
        Guid id,
        Guid customerId,
        string customerName,
        string shippingAddress)
    {
        Id = id;
        CustomerId = customerId;
        CustomerName = customerName;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
    }

    public static Order Create(
        Guid customerId,
        string customerName,
        string shippingAddress)
    {
        var order = new Order(
            Guid.NewGuid(),
            customerId,
            customerName,
            shippingAddress);
        return order;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var existingItem = OrderItems.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = new OrderItem(Id, productId, productName, unitPrice, quantity);
            _orderItems.Add(item);
        }
    }


    public void RemoveItem(Guid productId)
    {
        var item = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (item is not null)
        {
            _orderItems.Remove(item);
        }
    }

    public void MarkAsCreated()
    {
        RaiseDomainEvent(new OrderCreatedDomainEvent(Id, _orderItems.ToList()));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmedDomainEvent(Id));
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel shipped or delivered orders");

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledDomainEvent(Id));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed && Status != OrderStatus.Processing)
            throw new InvalidOperationException("Order must be confirmed before shipping");

        Status = OrderStatus.Shipped;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Order must be shipped before delivery");

        Status = OrderStatus.Delivered;
    }

    protected override void Apply(IDomainEvent @event)
    {
    }
}