using Marketing.Application.Posts.Dtos;
using Shared.Application.Abstractions.Messaging;

public record GetExternalDistributionByIdQuery(Guid DistributionId)
    : IQuery<ExternalDistributionDetailDto>;
