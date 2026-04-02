// Commands/MassRefundByEvent/MassRefundByEventCommandHandler.cs

using Payments.Application.Features.Refunds.Commands.MassRefundBySession;
using Payments.Application.Features.Refunds.Services;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Refunds.Commands.MassRefundByEvent;

public class MassRefundByEventCommandHandler(
    IPaymentTransactionRepository transactionRepository,
    MassRefundService massRefundService)
    : ICommandHandler<MassRefundByEventCommand, MassRefundResult>
{
    public async Task<Result<MassRefundResult>> Handle(
        MassRefundByEventCommand command, CancellationToken ct)
    {
        var pairs = await transactionRepository
            .GetAllCompletedItemsByEventIdAsync(command.EventId, ct);

        var result = await massRefundService.ExecuteAsync(
            scopeId:          command.EventId,
            pairs:            pairs,
            refundNotePrefix: $"Event refund | EventId={command.EventId}",
            ct:               ct);

        return Result.Success(result);
    }
}