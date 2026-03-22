using AutoMapper;
using Payment.Domain.Enums;
using Payments.Application.DTOs.Payment;
using Payments.Application.DTOs.Refund;
using Payments.Application.DTOs.Wallet;
using Payments.Application.Features.Payments.Commands.InitiatePayment;
using Payments.Application.Features.Payments.Commands.InitiateTopUp;
using Payments.Domain.Entities;

namespace Payments.Application.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        // --- PaymentTransaction → PaymentTransactionDto ---
        CreateMap<PaymentTransaction, PaymentTransactionDto>()
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.Items));

        CreateMap<BatchPaymentItem, BatchPaymentItemDto>();

        // --- Wallet → WalletDto ---
        CreateMap<Wallet, WalletDto>();

        CreateMap<Wallet, WalletWithTransactionsDto>()
            .ForMember(dest => dest.Transactions,
                opt => opt.MapFrom(src =>
                    src.Transactions.OrderByDescending(t => t.CreatedAt)));

        CreateMap<WalletTransaction, WalletTransactionDto>();

        // --- RefundRequest → RefundRequestDto ---
        CreateMap<RefundRequest, RefundRequestDto>();

        // --- Request bodies → Commands ---
        // UserId and IpAddress always come from ICurrentUserService in endpoint
        // — mapped there, never trusted from request body

        // CreateMap<InitiateTopUpRequest, InitiateTopUpCommand>()
        //     .ConstructUsing((src, _) => new InitiateTopUpCommand(
        //         UserId: Guid.Empty,
        //         IpAddress: string.Empty,
        //         Amount: src.Amount,
        //         Description: src.Description));

        // CreateMap<InitiatePaymentRequest, InitiatePaymentCommand>()
        //     .ConstructUsing((src, _) => new InitiatePaymentCommand(
        //         UserId: Guid.Empty,
        //         IpAddress: string.Empty,
        //         Method: src.Method,
        //         Items: src.Items,
        //         Description: src.Description));
    }
}
