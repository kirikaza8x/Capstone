using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<CreateCategoryCommand, int>
{
    public async Task<Result<int>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var codeExists = await categoryRepository.IsCodeExistsAsync(command.Code, cancellationToken);
        if (codeExists)
            return Result.Failure<int>(EventErrors.CategoryErrors.CodeAlreadyExists(command.Code));

        var category = Category.Create(command.Code, command.Name, command.Description);

        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }
}
