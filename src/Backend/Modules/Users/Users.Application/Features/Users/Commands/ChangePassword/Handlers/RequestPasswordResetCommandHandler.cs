using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.Handlers;

internal sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IUserUnitOfWork unitOfWork
) : ICommandHandler<RequestPasswordResetCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        
        var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
        
        
        if (user is null || !user.IsActive)
            return Result.Success(Guid.Empty); 

        user.CreateOtp();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}