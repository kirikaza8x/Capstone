using AI.Application.Features.AiPackages.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Queries.GetAiPackages;

public sealed record GetAiPackagesQuery : IQuery<IReadOnlyList<AiPackageDto>>;
