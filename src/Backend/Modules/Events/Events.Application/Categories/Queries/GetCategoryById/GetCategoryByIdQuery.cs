using Events.Application.Categories.Queries.GetCategories;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(int CategoryId) : IQuery<CategoryResponse>;
