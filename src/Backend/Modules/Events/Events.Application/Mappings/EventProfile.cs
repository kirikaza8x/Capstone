using AutoMapper;
using Events.Application.Events.DTOs;
using Events.Application.Events.Queries.GetEventById;
using Events.Application.Events.Queries.GetEvents;
using Events.Application.Events.Queries.GetEventsByOrganizer;
using Events.Application.Events.Queries.GetEventsForAdmin;
using Events.Application.Events.Queries.GetEventsForStaff;
using Events.Domain.Entities;

namespace Events.Application.Mappings;

public sealed class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<Event, GetEventResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Hashtags, opt => opt.MapFrom(src => src.EventHashtags))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.EventCategories))
            .ForMember(dest => dest.ActorImages, opt => opt.MapFrom(src => src.ActorImages))
            .ForMember(dest => dest.TicketTypes, opt => opt.MapFrom(src => src.TicketTypes));

        CreateMap<Event, EventResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.EventCategories))
            .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src => src.TicketTypes.Any() ? src.TicketTypes.Min(t => t.Price) : (decimal?)null))
            .ForMember(dest => dest.MaxPrice, opt => opt.MapFrom(src => src.TicketTypes.Any() ? src.TicketTypes.Max(t => t.Price) : (decimal?)null));

        CreateMap<Event, EventsByOrganizerResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Event, EventsForAdminResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())); ;
        CreateMap<Event, EventsForStaffResponse>()
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())); ;

        CreateMap<EventImage, EventImageDto>();
        CreateMap<EventActorImage, EventActorImageDto>();

        CreateMap<TicketType, TicketTypeDto>()
            .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Area != null ? src.Area.Name : null))
            .ForMember(dest => dest.AreaType, opt => opt.MapFrom(src => src.Area != null ? src.Area.Type.ToString() : null))
            .ForMember(dest => dest.RemainingQuantity, opt => opt.MapFrom(src => src.Quantity - src.SoldQuantity));

        CreateMap<EventSession, EventSessionDto>();

        CreateMap<EventHashtag, EventHashtagDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.HashtagId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Hashtag != null ? src.Hashtag.Name : string.Empty));

        CreateMap<EventCategory, EventCategoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
    }
}
