using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Marketing.Domain.Events;
using Marketing.Domain.Repositories;
using Marketing.Application.Services;
using AI.Domain.Interfaces.UOW;

namespace Marketing.Application.Features.Posts.EventHandlers;

public class NotifyN8nOfQueuedDistributionHandler(
    IPostRepository postRepository,
    IN8nDistributionService n8nService,
    IAiUnitOfWork unitOfWork,
    ILogger<NotifyN8nOfQueuedDistributionHandler> logger
) : IDomainEventHandler<PostQueuedForDistributionDomainEvent>
{
    public async Task Handle(PostQueuedForDistributionDomainEvent notification, CancellationToken cancellationToken)
    {
        // Load aggregate WITH child entities
        var post = await postRepository.GetByIdWithDistributionsAsync(
            notification.PostId, 
            cancellationToken);
        
        if (post is null)
        {
            logger.LogWarning("Post {PostId} not found for distribution notification", notification.PostId);
            return;
        }

        // Fire-and-forget: notify n8n + update status
        _ = Task.Run(async () =>
        {
            try
            {
                // 1. Call n8n service (service handles config, URL, HTTP)
                var sentToN8n = await n8nService.SendAsync(
                    post, 
                    notification.Platform, 
                    CancellationToken.None);

                // 2. If n8n accepted, update status via aggregate method
                if (sentToN8n)
                {
                    var result = post.MarkDistributionAsInProgress(notification.Platform);
                    if (result.IsSuccess)
                    {
                        postRepository.Update(post);
                        await unitOfWork.SaveChangesAsync(CancellationToken.None);
                        
                        logger.LogInformation("Post {PostId} → {Platform} marked as InProgress", 
                            post.Id, notification.Platform);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw — status stays Pending for retry/monitoring
                logger.LogError(ex, "Failed to notify n8n for Post {PostId}", post.Id);
            }
        }, CancellationToken.None);
    }
}