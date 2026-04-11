using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;
using Users.PublicApi.Constants;

namespace Ticketing.Application.Vouchers.Commands.DeleteVoucher;

internal sealed class DeleteVoucherCommandHandler(
    IVoucherRepository voucherRepository,
    ICurrentUserService currentUserService,
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<DeleteVoucherCommand>
{
    public async Task<Result> Handle(
        DeleteVoucherCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure(Error.Unauthorized(
                "DeleteVoucher.Unauthorized",
                "Current user is not authenticated."));

        var voucher = await voucherRepository.GetByIdAsync(
            command.VoucherId,
            cancellationToken);

        if (voucher is null)
            return Result.Failure(TicketingErrors.Voucher.NotFound(
                command.VoucherId.ToString()));

        var isAdmin = currentUserService.Roles.Contains(Roles.Admin);

        if (!isAdmin && voucher.CreatedBy != userId.ToString())
            return Result.Failure(TicketingErrors.Voucher.NotOwner);

        var canDelete = voucher.CanDelete();
        if (canDelete.IsFailure)
            return canDelete;

        voucherRepository.Remove(voucher);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
