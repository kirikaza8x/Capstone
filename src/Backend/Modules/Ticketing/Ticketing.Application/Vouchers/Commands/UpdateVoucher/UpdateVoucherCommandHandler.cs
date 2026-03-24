using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Data;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;
using Users.PublicApi.Constants;

namespace Ticketing.Application.Vouchers.Commands.UpdateVoucher;
public sealed class UpdateVoucherCommandValidator : AbstractValidator<UpdateVoucherCommand>
{
    public UpdateVoucherCommandValidator()
    {
        RuleFor(x => x.VoucherId)
            .NotEmpty().WithMessage("Voucher ID is required.");

        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(100).WithMessage("Coupon code must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid voucher type.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be greater than 0.");

        RuleFor(x => x.MaxUse)
            .GreaterThan(0).WithMessage("Max use must be greater than 0.");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
    }
}

internal sealed class UpdateVoucherCommandHandler(
    IVoucherRepository voucherRepository,
    ICurrentUserService currentUserService,
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<UpdateVoucherCommand>
{
    public async Task<Result> Handle(
        UpdateVoucherCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure(Error.Unauthorized(
                "UpdateVoucher.Unauthorized",
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

        var updateResult = voucher.Update(
            command.CouponCode,
            command.Type, 
            command.Value,
            command.MaxUse,
            command.StartDate,
            command.EndDate);

        if (updateResult.IsFailure)
            return updateResult;

        voucherRepository.Update(voucher);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
