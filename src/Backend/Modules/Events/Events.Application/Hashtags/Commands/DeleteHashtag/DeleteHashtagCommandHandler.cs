using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Hashtags.Commands.DeleteHashtag;

internal sealed class DeleteHashtagCommandHandler(
    IHashtagRepository hashtagRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<DeleteHashtagCommand>
{
    public async Task<Result> Handle(DeleteHashtagCommand command, CancellationToken cancellationToken)
    {
        var hashtag = await hashtagRepository.GetByIdAsync(command.HashtagId, cancellationToken);
        if (hashtag is null)
            return Result.Failure(EventErrors.HashtagErrors.NotFound(command.HashtagId));

        hashtagRepository.Remove(hashtag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}