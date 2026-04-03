using AI.Domain.Enums;

namespace AI.Application.Features.AiPackages.Dtos;

public sealed record MyPurchasedAiPackageDto(
    Guid PackageId,
    string Name,
    string? Description,
    AiPackageType Type,
    decimal Price,
    int TokenQuota,
    bool IsActive,
    int PurchaseCount,
    int TotalPurchasedTokens,
    DateTime LastPurchasedAt);
