using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Abstractions.Sms;
using Users.Application.Features.Users.Commands.Records;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.Handlers;

internal sealed class RequestPasswordResetCommandHandler(
    IUserNotificationService notificationService,
    IUserRepository userRepository,
    IUserUnitOfWork unitOfWork
) : ICommandHandler<RequestPasswordResetCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {

        var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);


        if (user is null || !user.IsActive || user.Email == null)
            return Result.Success(Guid.Empty);

        var otp = user.CreateOtp();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notificationService.SendOtpAsync(user.Id, user.Email, otp.OtpCode);
        return Result.Success(user.Id);
    }
}