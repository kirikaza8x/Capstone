using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Categories.Commands.ToggleCategoryStatus;

internal sealed class ToggleCategoryStatusCommandHandler(
    ICategoryRepository categoryRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<ToggleCategoryStatusCommand>
{
    public async Task<Result> Handle(ToggleCategoryStatusCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(EventErrors.CategoryErrors.NotFound(command.CategoryId));

        if (command.Activate)
            category.Activate();
        else
            category.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
