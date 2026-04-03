using AI.Application.Features.AiPackages.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Queries.GetMyPurchasedAiPackages;

public sealed record GetMyPurchasedAiPackagesQuery : IQuery<IReadOnlyList<MyPurchasedAiPackageDto>>;
