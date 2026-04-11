using AutoMapper;
using Payment.Domain.Enums;
using Payments.Application.DTOs.Payment;
using Payments.Application.DTOs.Refund;
using Payments.Application.DTOs.Wallet;
using Payments.Application.Features.Payments.Commands.InitiatePayment;
using Payments.Application.Features.Payments.Commands.InitiateTopUp;
using Payments.Domain.Entities;
using Users.PublicApi.PublicApi;

namespace Payments.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentTransaction, PaymentTransactionDto>()
    .ForMember(dest => dest.Username, opt => opt.MapFrom<UsernameResolver<PaymentTransactionDto>>());

        CreateMap<PaymentTransaction, PaymentTransactionDetailDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom<UsernameResolver<PaymentTransactionDetailDto>>());

        CreateMap<BatchPaymentItem, BatchPaymentItemDto>();

        // --- Wallet → WalletDto ---
        CreateMap<Wallet, WalletDto>();

        CreateMap<Wallet, WalletWithTransactionsDto>()
            .ForMember(dest => dest.Transactions,
                opt => opt.MapFrom(src =>
                    src.Transactions.OrderByDescending(t => t.CreatedAt)));

        CreateMap<WalletTransaction, WalletTransactionDto>();

        CreateMap<RefundRequest, RefundRequestDto>();

    }
}

public class UsernameResolver<TDestination>
    : IValueResolver<PaymentTransaction, TDestination, string?>
{
    public string? Resolve(
        PaymentTransaction source,
        TDestination destination,
        string? destMember,
        ResolutionContext context)
    {
        if (context.Items.TryGetValue("userMap", out var mapObj) &&
            mapObj is Dictionary<Guid, UserInfo> userMap &&
            userMap.TryGetValue(source.UserId, out var userInfo))
        {
            return userInfo.Username;
        }

        return null;
    }
}



