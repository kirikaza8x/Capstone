using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Categories.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(EventErrors.CategoryErrors.NotFound(command.CategoryId));

        category.Update(command.Name, command.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}