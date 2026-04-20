using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Marketing.Domain.Events;
using Marketing.Domain.Repositories;
using Marketing.Application.Services;

namespace Marketing.Application.Features.Posts.EventHandlers;

public class NotifyN8nOfQueuedDistributionHandler(
    IPostRepository postRepository,
    IN8nDistributionService n8nService,
    ILogger<NotifyN8nOfQueuedDistributionHandler> logger
) : IDomainEventHandler<PostQueuedForDistributionDomainEvent>
{
    public async Task Handle(PostQueuedForDistributionDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("NotifyN8n started: postId={PostId} platform={Platform}",
            notification.PostId, notification.Platform);

        var post = await postRepository.GetByIdWithDistributionsAsync(
            notification.PostId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Post {PostId} not found for distribution notification", notification.PostId);
            return;
        }

        try
        {
            var sentToN8n = await n8nService.SendAsync(
                post,
                notification.Platform,
                cancellationToken);

            if (sentToN8n)
            {
                logger.LogInformation("n8n accepted request for Post {PostId} → {Platform}",
                    notification.PostId, notification.Platform);
            }
            else
            {
                logger.LogWarning("n8n rejected the request for Post {PostId} → {Platform}",
                    notification.PostId, notification.Platform);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify n8n for Post {PostId}", post.Id);
        }
    }
}