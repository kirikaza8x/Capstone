using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Marketing.Domain.Events;
using Marketing.Domain.Repositories;
using Marketing.Application.Services;
using System.Diagnostics;

namespace Marketing.Application.Features.Posts.EventHandlers;

public class NotifyN8nOfQueuedDistributionHandler(
    IPostRepository postRepository,
    IN8nDistributionService n8nService,
    ILogger<NotifyN8nOfQueuedDistributionHandler> logger) : IDomainEventHandler<PostQueuedForDistributionDomainEvent>
{
    private static readonly HashSet<Guid> _processedEvents = [];
    private static readonly ActivitySource ActivitySource = new("Marketing.N8n");

    public async Task Handle(PostQueuedForDistributionDomainEvent notification, CancellationToken cancellationToken)
    {
        // Deduplicate at handler level to prevent duplicate n8n calls
        if (!_processedEvents.Add(notification.EventId))
        {
            logger.LogWarning(
                "Duplicate event detected at handler level, skipping: EventId={EventId} PostId={PostId}",
                notification.EventId, notification.PostId);
            return;
        }

        using var activity = ActivitySource.StartActivity("NotifyN8n");
        activity?.SetTag("PostId", notification.PostId.ToString());
        activity?.SetTag("Platform", notification.Platform.ToString());
        activity?.SetTag("EventId", notification.EventId.ToString());

        logger.LogInformation(
            "NotifyN8n started: postId={PostId} platform={Platform} eventId={EventId}",
            notification.PostId, notification.Platform, notification.EventId);

        var post = await postRepository.GetByIdWithDistributionsAsync(
            notification.PostId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Post {PostId} not found for distribution notification", notification.PostId);
            return;
        }

        logger.LogInformation(
            "Post found: id={Id}, status={Status}, distributionCount={Count}",
            post.Id, post.Status, post.ExternalDistributions.Count);

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