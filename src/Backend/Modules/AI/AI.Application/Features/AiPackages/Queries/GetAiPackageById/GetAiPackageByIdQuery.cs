using AI.Application.Features.AiPackages.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Queries.GetAiPackageById;

public sealed record GetAiPackageByIdQuery(Guid Id) : IQuery<AiPackageDto>;
