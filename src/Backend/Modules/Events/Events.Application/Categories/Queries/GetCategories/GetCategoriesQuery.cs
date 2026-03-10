using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Categories.Queries.GetCategories;

public sealed record CategoryResponse(int Id, string Code, string Name, string? Description, bool IsActive);

public sealed record GetCategoriesQuery : IQuery<IReadOnlyList<CategoryResponse>>;