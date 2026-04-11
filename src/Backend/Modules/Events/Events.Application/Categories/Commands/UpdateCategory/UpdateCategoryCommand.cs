using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    int CategoryId,
    string Code,
    string Name,
    string? Description) : ICommand;
