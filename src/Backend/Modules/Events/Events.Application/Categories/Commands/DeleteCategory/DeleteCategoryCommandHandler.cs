using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Categories.Commands.DeleteCategory;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(EventErrors.CategoryErrors.NotFound(command.CategoryId));

        categoryRepository.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}