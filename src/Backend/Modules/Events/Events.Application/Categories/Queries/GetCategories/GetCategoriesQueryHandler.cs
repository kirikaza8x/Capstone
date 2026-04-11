using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Categories.Queries.GetCategories;

internal sealed class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository) : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = string.IsNullOrWhiteSpace(query.Name)
            ? await categoryRepository.GetAllAsync(cancellationToken)
            : await categoryRepository.SearchAsync(c => c.Name, query.Name, query.Take, cancellationToken);

        var response = categories
            .Select(c => new CategoryResponse(c.Id, c.Code, c.Name, c.Description, c.IsActive))
            .ToList();

        return Result.Success<IReadOnlyList<CategoryResponse>>(response);
    }
}
