using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Shared.Domain.Data;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;
using Users.PublicApi.Constants;

namespace Ticketing.Application.Vouchers.Commands.CreateVoucher;

public sealed class CreateVoucherCommandValidator : AbstractValidator<CreateVoucherCommand>
{
    public CreateVoucherCommandValidator()
    {
        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(100).WithMessage("Coupon code must not exceed 100 characters.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be greater than 0.");

        RuleFor(x => x.MaxUse)
            .GreaterThan(0).WithMessage("Max use must be greater than 0.");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
    }
}
internal sealed class CreateVoucherCommandHandler(
    IVoucherRepository voucherRepository,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<CreateVoucherCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateVoucherCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Result.Failure<Guid>(Error.Unauthorized(
                "CreateVoucher.Unauthorized",
                "Current user is not authenticated."));

        var currentUser = currentUserService.GetCurrentUser();

        var isAdmin = currentUser.Roles.Contains(Roles.Admin);
        var isOrganizer = currentUser.Roles.Contains(Roles.Organizer);


        if (!isAdmin && isOrganizer && !command.EventId.HasValue)
        {
            return Result.Failure<Guid>(Error.Forbidden(
                "CreateVoucher.Forbidden",
                "Organizer must specify an event for the voucher."));
        }

        // Check duplicate coupon code
        var exists = await voucherRepository.IsCouponCodeExistsAsync(
            command.CouponCode,
            cancellationToken);

        if (exists)
            return Result.Failure<Guid>(TicketingErrors.Voucher.CouponCodeAlreadyExists(command.CouponCode));

        var voucherResult = Voucher.Create(
            command.CouponCode,
            command.Type,
            command.Value,
            command.MaxUse,
            command.StartDate,
            command.EndDate,
            command.EventId,
            dateTimeProvider.UtcNow);

        if (voucherResult.IsFailure)
            return Result.Failure<Guid>(voucherResult.Error);

        voucherRepository.Add(voucherResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(voucherResult.Value.Id);
    }
}
