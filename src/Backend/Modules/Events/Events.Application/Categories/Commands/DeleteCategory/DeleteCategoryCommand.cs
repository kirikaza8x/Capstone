using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(int CategoryId) : ICommand;