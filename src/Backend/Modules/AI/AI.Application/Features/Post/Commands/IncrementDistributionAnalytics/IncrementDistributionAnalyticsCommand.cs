using Shared.Domain.Abstractions;
using Marketing.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record IncrementDistributionAnalyticsCommand(
    Guid PostId,
    ExternalPlatform Platform,
    Guid? DistributionId = null,
    int BuyIncrement = 0,
    int ClickIncrement = 0) : ICommand;