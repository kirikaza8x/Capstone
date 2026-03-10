using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Categories.Commands.ToggleCategoryStatus;

public sealed record ToggleCategoryStatusCommand(int CategoryId, bool Activate) : ICommand;