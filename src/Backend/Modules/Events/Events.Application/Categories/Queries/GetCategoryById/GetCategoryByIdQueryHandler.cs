using Events.Application.Categories.Queries.GetCategories;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Categories.Queries.GetCategoryById;

internal sealed class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository) : IQueryHandler<GetCategoryByIdQuery, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(query.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryResponse>(EventErrors.CategoryErrors.NotFound(query.CategoryId));

        return Result.Success(new CategoryResponse(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.IsActive));
    }
}
