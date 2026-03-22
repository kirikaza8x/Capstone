using Payments.Application.DTOs.Wallet;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Queries.GetMyWallet;

public class GetMyWalletQueryHandler(
    ICurrentUserService currentUser,
    IWalletRepository walletRepository)
    : IQueryHandler<GetMyWalletQuery, WalletWithTransactionsDto>
{
    public async Task<Result<WalletWithTransactionsDto>> Handle(
        GetMyWalletQuery query, CancellationToken cancellationToken)
    {
        
        var wallet = await walletRepository
            .GetByUserIdWithTransactionsAsync(
                currentUser.UserId, query.TransactionLimit, cancellationToken);

        if (wallet == null)
            return Result.Failure<WalletWithTransactionsDto>(
                Error.NotFound("Wallet.NotFound", "No wallet found for this user."));

        var dto = new WalletWithTransactionsDto(
            Id: wallet.Id,
            UserId: wallet.UserId,
            Balance: wallet.Balance,
            Status: wallet.Status,
            Transactions: wallet.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto(
                    Id: t.Id,
                    Type: t.Type,
                    Direction: t.Direction,
                    Amount: t.Amount,
                    BalanceBefore: t.BalanceBefore,
                    BalanceAfter: t.BalanceAfter,
                    Status: t.Status,
                    Note: t.Note,
                    CreatedAt: t.CreatedAt))   // DateTime? — matches updated DTO
                .ToList(),
            CreatedAt: wallet.CreatedAt);       // DateTime? — matches updated DTO

        return Result.Success(dto);
    }
}