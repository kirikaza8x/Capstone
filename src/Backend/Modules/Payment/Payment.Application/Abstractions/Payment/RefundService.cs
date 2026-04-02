// using Payments.Domain.Entities;


// public interface IMassRefundProcessor
// {
//     Task<MassRefundResult> ProcessAsync(
//         IEnumerable<BatchPaymentItem> itemsToRefund,
//         string refundReason,
//         CancellationToken ct);
// }

// public record MassRefundResult(
//     int TotalProcessed,
//     int TotalSucceeded,
//     int TotalSkipped,
//     int TotalFailed,
//     List<string> Errors);