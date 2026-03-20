using AutoMapper;
using Payment.Application.Features.VnPay.Dtos;
using Payment.Domain.Enums;
using Payments.Domain.Entities;

namespace Payments.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        // --- Request → Command ---

        // CreateMap<InitiatePaymentRequest, InitiatePaymentCommand>()
        //     .ConstructUsing((src, ctx) => new InitiatePaymentCommand(
        //         UserId: Guid.Empty,       // overwritten in endpoint from ICurrentUserService
        //         IpAddress: string.Empty,  // overwritten in endpoint from ICurrentUserService
        //         Amount: src.Amount,
        //         Type: src.Type,
        //         EventId: src.EventId,
        //         Description: src.Description
        //     ));

        // CreateMap<WalletPayRequest, WalletPayCommand>()
        //     .ConstructUsing((src, ctx) => new WalletPayCommand(
        //         EventId: src.EventId,
        //         Amount: src.Amount,
        //         Description: src.Description
        //     ));

        // --- PaymentTransaction → DTOs ---

        CreateMap<PaymentTransaction, VnPayResultDto>()
            .ForMember(dest => dest.ItemId,
                opt => opt.MapFrom(src => src.EventId ?? src.WalletId ?? Guid.Empty))
            .ForMember(dest => dest.PaymentSuccess,
                opt => opt.MapFrom(src => src.InternalStatus == PaymentInternalStatus.Completed))
            .ForMember(dest => dest.PaymentMessage,
                opt => opt.MapFrom(src => src.GatewayOrderInfo))
            .ForMember(dest => dest.TransactionNo,
                opt => opt.MapFrom(src => src.GatewayTransactionNo))
            .ForMember(dest => dest.ResponseCode,
                opt => opt.MapFrom(src => src.GatewayResponseCode))
            .ForMember(dest => dest.CheckedOutAt,
                opt => opt.MapFrom(src => src.CompletedAt ?? DateTime.UtcNow));

        CreateMap<PaymentTransaction, RefundByEventResultDto>()
            .ForMember(dest => dest.PaymentTransactionId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AmountRefunded,
                opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.WalletBalanceAfter,
                opt => opt.Ignore())
            .ForMember(dest => dest.RefundedAt,
                opt => opt.MapFrom(src => src.RefundedAt ?? DateTime.UtcNow));

        CreateMap<PaymentTransaction, MassRefundItemResult>()
            .ForMember(dest => dest.PaymentTransactionId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Success,
                opt => opt.MapFrom(src => src.InternalStatus == PaymentInternalStatus.Refunded))
            .ForMember(dest => dest.FailureReason,
                opt => opt.Ignore());     // set manually on failure path

        // --- WalletTransaction → DTOs ---

        CreateMap<WalletTransaction, WalletPayResultDto>()
            .ForMember(dest => dest.PaymentTransactionId,
                opt => opt.Ignore())      // set from PaymentTransaction, not WalletTransaction
            .ForMember(dest => dest.WalletTransactionId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AmountDebited,
                opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.BalanceAfter,
                opt => opt.MapFrom(src => src.BalanceAfter))
            .ForMember(dest => dest.PaidAt,
                opt => opt.MapFrom(src => src.CreatedAt));
    }
}
