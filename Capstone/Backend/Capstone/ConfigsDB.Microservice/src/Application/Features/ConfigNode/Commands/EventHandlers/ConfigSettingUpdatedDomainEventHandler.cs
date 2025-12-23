// using MediatR;
// using ConfigsDB.Application.Abstractions.Configs;
// using ConfigsDB.Domain.Events;
// using Shared.Application.Abstractions.Messaging;

// namespace ConfigsDB.Application.Features.ConfigSettings.Events;

// // This class is triggered by 'await mediator.Publish(domainEvent)' 
// // inside your Interceptor!
// public class ConfigSettingUpdatedDomainEventHandler 
//     : IEventHandler<ConfigSettingUpdatedDomainEvent>
// {
//     private readonly IConfigDistributor _distributor;

//     public ConfigSettingUpdatedDomainEventHandler(IConfigDistributor distributor)
//     {
//         _distributor = distributor;
//     }

//     public async Task Handle(ConfigSettingUpdatedDomainEvent notification, CancellationToken ct)
//     {
//         // This is where we bridge Domain Logic to Infrastructure (RabbitMQ)
//         // We use the Strategy pattern we built earlier.
//         await _distributor.DistributeAsync(notification.Key, notification.Environment, ct);
//     }
// }