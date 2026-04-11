using AI.Application.Features.AiPackages.Dtos;
using AI.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Commands.UpdateAiPackage;

public sealed record UpdateAiPackageCommand(
    Guid Id,
    string Name,
    string? Description,
    AiPackageType Type,
    decimal Price,
    int TokenQuota,
    bool IsActive) : ICommand<AiPackageDto>;
