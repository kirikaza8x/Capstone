using Payment.Application.Features.Vnpay.DTOs;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetRevenueByEventQueryHandler(
    IPaymentTransactionRepository repo)
    : IQueryHandler<GetRevenueByEventQuery, EventRevenueDto>
{
    public async Task<Result<EventRevenueDto>> Handle(
        GetRevenueByEventQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repo.GetRevenueByEventAsync(
            query.EventId, cancellationToken);

        if (result is null)
        {
            return Result.Failure<EventRevenueDto>(
                Error.NotFound(
                    "Revenue.NotFound",
                    "No revenue found for this event."));
        }

        var dto = new EventRevenueDto(
            result.EventId,
            result.Revenue);

        return Result.Success(dto);
    }
}