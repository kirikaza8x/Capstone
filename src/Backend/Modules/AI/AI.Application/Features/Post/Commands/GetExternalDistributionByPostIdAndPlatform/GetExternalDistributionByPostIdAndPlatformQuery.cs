using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

public record GetExternalDistributionByPostIdAndPlatformQuery(Guid PostId, ExternalPlatform platForm)
    : IQuery<ExternalDistributionDetailDto>;
