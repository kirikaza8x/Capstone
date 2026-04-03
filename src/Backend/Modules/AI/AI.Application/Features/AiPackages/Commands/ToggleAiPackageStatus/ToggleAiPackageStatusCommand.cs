using AI.Application.Features.AiPackages.Dtos;
using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Commands.ToggleAiPackageStatus;

public sealed record ToggleAiPackageStatusCommand(Guid Id) : ICommand<AiPackageDto>;
