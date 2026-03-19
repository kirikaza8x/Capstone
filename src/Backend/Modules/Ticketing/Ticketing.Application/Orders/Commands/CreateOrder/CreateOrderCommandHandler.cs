//using FluentValidation;
//using Shared.Application.Abstractions.Authentication;
//using Shared.Application.Abstractions.Messaging;
//using Shared.Domain.Abstractions;
//using Ticketing.Domain.Entities;
//using Ticketing.Domain.Repositories;
//using Ticketing.Domain.Uow;

//namespace Ticketing.Application.Orders.Commands.CreateOrder;

//public sealed class CreateOrderTicketItemValidator : AbstractValidator<CreateOrderTicketItem>
//{
//    public CreateOrderTicketItemValidator()
//    {
//        RuleFor(x => x.EventSessionId).NotEmpty();
//        RuleFor(x => x.TicketTypeId).NotEmpty();
//    }
//}

//public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
//{
//    public CreateOrderCommandValidator()
//    {
//        RuleFor(x => x.Tickets)
//            .NotEmpty();

//        RuleForEach(x => x.Tickets)
//            .SetValidator(new CreateOrderTicketItemValidator());

//        RuleFor(x => x.Tickets)
//            .Must(items =>
//            {
//                var seatIds = items
//                    .Where(i => i.SeatId.HasValue)
//                    .Select(i => i.SeatId!.Value)
//                    .ToList();

//                return seatIds.Count == seatIds.Distinct().Count();
//            })
//            .WithMessage("Duplicate seat_id in order tickets is not allowed.");
//    }
//}

//internal sealed class CreateOrderCommandHandler(
//    ICurrentUserService currentUserService,
//    IOrderRepository orderRepository,
//    ITicketingUnitOfWork unitOfWork) : ICommandHandler<CreateOrderCommand, Guid>
//{
//    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
//    {
//        if (currentUserService.UserId == Guid.Empty)
//            return Result.Failure<Guid>(Error.Unauthorized(
//                "Order.Create.Unauthorized",
//                "Current user is not authenticated."));

//        var order = Order.Create(
//            userId: currentUserService.UserId);

//        foreach (var item in command.Tickets)
//        {
//            var qrCode = Guid.NewGuid().ToString("N");

//            var addTicketResult = order.AddTicket(
//                eventSessionId: item.EventSessionId,
//                ticketTypeId: item.TicketTypeId,
//                seatId: item.SeatId,
//                qrCode: qrCode);

//            if (addTicketResult.IsFailure)
//                return Result.Failure<Guid>(addTicketResult.Error);
//        }

//        //var totalPrice = command.Tickets.Sum(x => x.TicketPrice);
//        var setTotalResult = order.SetTotalPrice(totalPrice);

//        if (setTotalResult.IsFailure)
//            return Result.Failure<Guid>(setTotalResult.Error);

//        orderRepository.Add(order);
//        await unitOfWork.SaveChangesAsync(cancellationToken);

//        return Result.Success(order.Id);
//    }
//}