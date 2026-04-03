using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.AiPackages.Commands.DeleteAiPackage;

public sealed record DeleteAiPackageCommand(Guid Id) : ICommand;
