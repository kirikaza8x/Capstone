using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Code,
    string Name,
    string? Description) : ICommand<int>;