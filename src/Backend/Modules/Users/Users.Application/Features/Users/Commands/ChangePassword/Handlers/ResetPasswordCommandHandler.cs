using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Domain.Errors.Otp;
using Users.Domain.Errors.Users;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.Handlers;

internal sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IUserUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher
) : ICommandHandler<ResetPasswordCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailOtpAsync(command.Email, cancellationToken);

        if (user is null || !user.IsActive)
            return Result.Failure<Guid>(UserErrors.NotFound);

        try
        {
            var hashedPassword = passwordHasher.HashPassword(command.NewPassword);

            user.ResetPassword(command.OtpCode, hashedPassword);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(user.Id);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<Guid>(OtpErrors.InvalidCode);
        }
    }
}
