// using Microsoft.Extensions.Logging;
// using Shared.Application.Abstractions.EventBus;
// using Shared.Application.Abstractions.Messaging;
// using AI.Domain.Event;
// using Shared.IntegrationEvents.AI;
// // using Shared.IntegrationEvents.AI; // Where your UserActivityTrackedIntegrationEvent lives

// namespace AI.Application.Features.Tracking.EventHandlers
// {
//     /// <summary>
//     /// Handles the CreateLog domain event.
//     /// Publishes an integration event to notify other services (e.g., Analytics) 
//     /// that a user behavior log was recorded.
//     /// </summary>
//     public class CreateLogEventHandler : IDomainEventHandler<CreateLogEvent>
//     {
//         private readonly IEventBus _eventBus;
//         private readonly ILogger<CreateLogEventHandler> _logger;

//         public CreateLogEventHandler(IEventBus eventBus, ILogger<CreateLogEventHandler> logger)
//         {
//             _eventBus = eventBus;
//             _logger = logger;
//         }

//         public async Task Handle(CreateLogEvent notification, CancellationToken cancellationToken)
//         {
//             try
//             {
//                 _logger.LogInformation(
//                     "Handling log creation for UserId: {UserId}, Action: {ActionType}", 
//                     notification.UserId, 
//                     notification.ActionType);

//                 var integrationEvent = new TrackUserActivityIntegrationEvent(
//                     Id: notification.EventId, 
//                     UserId: notification.UserId,
//                     ActionType: notification.ActionType,
//                     TargetId: notification.TargetId,
//                     TargetType: notification.TargetType,
//                     Metadata: notification.Metadata,
//                     OccurredOnUtc: notification.OccurredOn
//                 );

//                 await _eventBus.PublishAsync(integrationEvent, cancellationToken);

//                 _logger.LogInformation(
//                     "Integration event published for UserId: {UserId}, Target: {TargetId}", 
//                     notification.UserId, 
//                     notification.TargetId);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(
//                     ex, 
//                     "Failed to handle log creation domain event for {UserId}", 
//                     notification.UserId);
//                 throw;
//             }
//         }
//     }
// }