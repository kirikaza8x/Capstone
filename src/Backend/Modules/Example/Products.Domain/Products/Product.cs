using Products.Domain.Products.Events;
using Shared.Domain.DDD;

namespace Products.Domain.Products;

public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public int Stock { get; private set; }

    private Product() { }

    private Product(
     Guid id,
     string name,
     string description,
     decimal price,
     int stock)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
    }

    public static Product Create(
       string name,
       string description,
       decimal price,
       int stock)
    {
        var product = new Product(
            Guid.NewGuid(),
            name,
            description,
            price,
            stock);

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id));

        return product;
    }

    public void Update(string name, string description, decimal price, int stock)
    {
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
    }

    public void UpdateStock(int quantity)
    {
        Stock += quantity;
    }

    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
    }

    public void Delete()
    {
        IsActive = false;
    }

    protected override void Apply(IDomainEvent @event)
    {
    }
}
