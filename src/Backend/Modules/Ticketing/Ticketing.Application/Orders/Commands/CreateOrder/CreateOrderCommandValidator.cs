using FluentValidation;

namespace Ticketing.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderTicketItemValidator : AbstractValidator<CreateOrderTicketItem>
{
    public CreateOrderTicketItemValidator()
    {
        RuleFor(x => x.EventSessionId).NotEmpty();
        RuleFor(x => x.TicketTypeId).NotEmpty();
    }
}

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Tickets).NotEmpty();

        RuleForEach(x => x.Tickets)
            .SetValidator(new CreateOrderTicketItemValidator());

        RuleFor(x => x.Tickets)
            .Must(items =>
            {
                var seatIds = items
                    .Where(i => i.SeatId.HasValue)
                    .Select(i => i.SeatId!.Value)
                    .ToList();

                return seatIds.Count == seatIds.Distinct().Count();
            })
            .WithMessage("Duplicate seat_id in order tickets is not allowed.");
    }
}