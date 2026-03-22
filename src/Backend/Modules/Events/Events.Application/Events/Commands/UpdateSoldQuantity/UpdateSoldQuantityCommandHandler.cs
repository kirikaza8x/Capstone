using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.UpdateSoldQuantity;

internal sealed class UpdateSoldQuantityCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateSoldQuantityCommand>
{
    public async Task<Result> Handle(
        UpdateSoldQuantityCommand command,
        CancellationToken cancellationToken)
    {
        var ticketTypeIds = command.Items.Select(i => i.TicketTypeId).ToList();
        var quantityMap = command.Items.ToDictionary(i => i.TicketTypeId, i => i.Quantity);

        var ticketTypes = await eventRepository.GetTicketTypesByIdsAsync(
            ticketTypeIds,
            cancellationToken);

        foreach (var ticketType in ticketTypes)
            ticketType.IncreaseSoldQuantity(quantityMap[ticketType.Id]);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
