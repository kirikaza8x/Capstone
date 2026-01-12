using Shared.Domain.DDD;

namespace Products.Domain.Products.Events;

public sealed record ProductCreatedDomainEvent(Guid ProductId) : DomainEvent;
