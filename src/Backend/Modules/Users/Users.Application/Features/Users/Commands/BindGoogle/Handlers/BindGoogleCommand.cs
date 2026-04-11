using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Abstractions.Authentication;
using Users.Domain.Repositories;
using Users.Domain.UOW;

public class BindGoogleCommandHandler(
    IUserRepository userRepository,
    IGooglePayloadValidator googleValidator,
    ICurrentUserService currentUserService,
    IUserUnitOfWork unitOfWork
) : ICommandHandler<BindGoogleCommand>
{
    public async Task<Result> Handle(BindGoogleCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(currentUserService.UserId, cancellationToken);
        if (user == null) return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        var payloadResult = await googleValidator.ValidateAsync(command.IdToken);
        if (payloadResult.IsFailure) return payloadResult;
        var payload = payloadResult.Value;

        var existingLink = await userRepository.GetByExternalIdentityAsync("Google", payload.Subject, cancellationToken);
        if (existingLink != null && existingLink.Id != user.Id)
        {
            return Result.Failure(Error.Conflict("External.Conflict", "This Google account is already linked to another user."));
        }

        user.BindExternalIdentity("Google", payload.Subject);
        user.Verify();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
