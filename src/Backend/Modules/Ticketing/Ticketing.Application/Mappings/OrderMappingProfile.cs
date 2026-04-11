using AutoMapper;
using Ticketing.Application.Orders.Queries.GetMyOrders;
using Ticketing.Application.Vouchers.Queries.Dto;
using Ticketing.Application.Vouchers.Queries.GetVouchers;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Mappings;

public sealed class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, MyOrderResponse>()
            .ForMember(dest => dest.OrderId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalTickets,
                opt => opt.MapFrom(src => src.Tickets.Count))
            .ForMember(dest => dest.DiscountAmount,
                opt => opt.MapFrom(src =>
                    src.OrderVouchers.Any()
                        ? src.OrderVouchers.Sum(ov => ov.DiscountAmount)
                        : (decimal?)null));

        CreateMap<Voucher, VoucherDto>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.IsGlobal,
                opt => opt.MapFrom(src => src.EventId == null));
    }
}
