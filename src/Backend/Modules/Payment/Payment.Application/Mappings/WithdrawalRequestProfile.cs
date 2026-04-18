using AutoMapper;
using Payments.Application.Features.WithdrawalRequests.Dtos;
using Payments.Domain.Entities;

namespace Payments.Application.Features.WithdrawalRequests.Mappings;

public class WithdrawalRequestProfile : Profile
{
    public WithdrawalRequestProfile()
    {
        // ========================
        // USER LIST
        // ========================
        CreateMap<WithdrawalRequest, WithdrawalRequestListItemDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // ========================
        // USER DETAIL
        // ========================
        CreateMap<WithdrawalRequest, WithdrawalRequestDetailDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // ========================
        // ADMIN LIST
        // ========================
        CreateMap<WithdrawalRequest, WithdrawalRequestAdminListItemDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ReceiverName, o => o.MapFrom(s => s.Name))
            ;

        // ========================
        // ADMIN DETAIL
        // ========================
        CreateMap<WithdrawalRequest, WithdrawalRequestAdminDetailDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}