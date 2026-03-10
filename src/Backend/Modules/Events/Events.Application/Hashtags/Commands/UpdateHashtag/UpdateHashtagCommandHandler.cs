using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Hashtags.Commands.UpdateHashtag;

internal sealed class UpdateHashtagCommandHandler(
    IHashtagRepository hashtagRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateHashtagCommand>
{
    public async Task<Result> Handle(UpdateHashtagCommand command, CancellationToken cancellationToken)
    {
        var hashtag = await hashtagRepository.GetByIdAsync(command.HashtagId, cancellationToken);
        if (hashtag is null)
            return Result.Failure(EventErrors.HashtagErrors.NotFound(command.HashtagId));

        hashtag.UpdateName(command.Name);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}