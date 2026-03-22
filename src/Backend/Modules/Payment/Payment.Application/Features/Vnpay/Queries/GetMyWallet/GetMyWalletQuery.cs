using Payments.Application.DTOs.Wallet;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Queries.GetMyWallet;

public record GetMyWalletQuery(
    // Guid UserId,
    int TransactionLimit = 10
) : IQuery<WalletWithTransactionsDto>;