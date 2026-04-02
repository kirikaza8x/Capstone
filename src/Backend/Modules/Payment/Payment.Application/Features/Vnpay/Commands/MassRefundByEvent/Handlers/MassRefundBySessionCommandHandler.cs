
using Payments.Application.Features.Refunds.Services;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Refunds.Commands.MassRefundBySession;

public class MassRefundBySessionCommandHandler(
    IPaymentTransactionRepository transactionRepository,
    MassRefundService massRefundService)
    : ICommandHandler<MassRefundBySessionCommand, MassRefundResult>
{
    public async Task<Result<MassRefundResult>> Handle(
        MassRefundBySessionCommand command, CancellationToken ct)
    {
        var pairs = await transactionRepository
            .GetAllCompletedItemsBySessionIdAsync(command.EventSessionId, ct);

        var result = await massRefundService.ExecuteAsync(
            scopeId: command.EventSessionId,
            pairs: pairs,
            refundNotePrefix: $"Session refund | SessionId={command.EventSessionId}",
            ct: ct);

        return Result.Success(result);
    }
}