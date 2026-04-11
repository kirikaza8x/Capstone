using AI.Domain.Enums;

namespace AI.Application.Features.AiPackages.Dtos;

public sealed record AiPackageDto(
    Guid Id,
    string Name,
    string? Description,
    AiPackageType Type,
    decimal Price,
    int TokenQuota,
    bool IsActive);
