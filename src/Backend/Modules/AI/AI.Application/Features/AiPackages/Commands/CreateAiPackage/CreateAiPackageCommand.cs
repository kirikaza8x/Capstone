using AI.Application.Features.AiPackages.Dtos;
using AI.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Commands.CreateAiPackage;

public sealed record CreateAiPackageCommand(
    string Name,
    string? Description,
    AiPackageType Type,
    decimal Price,
    int TokenQuota) : ICommand<AiPackageDto>;
