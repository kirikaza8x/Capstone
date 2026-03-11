using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Helpers;
using Shared.Domain.Abstractions;

namespace Events.Application.Hashtags.Commands.CreateHashtag;

internal sealed class CreateHashtagCommandHandler(
    IHashtagRepository hashtagRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<CreateHashtagCommand, int>
{
    public async Task<Result<int>> Handle(CreateHashtagCommand command, CancellationToken cancellationToken)
    {
        var slug = SlugHelper.Generate(command.Name);

        var slugExists = await hashtagRepository.IsSlugExistsAsync(slug, cancellationToken);
        if (slugExists)
            return Result.Failure<int>(EventErrors.HashtagErrors.SlugAlreadyExists(slug));

        var hashtag = Hashtag.Create(command.Name, slug);

        hashtagRepository.Add(hashtag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(hashtag.Id);
    }
}