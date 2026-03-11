using AutoMapper;
using Events.Application.Events.DTOs;
using Events.Application.Events.Queries.GetEventById;
using Events.Application.Events.Queries.GetEvents;
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
            .ForMember(dest => dest.ActorImages, opt => opt.MapFrom(src => src.ActorImages));

        CreateMap<Event, EventResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.EventCategories));

        CreateMap<EventImage, EventImageDto>();
        CreateMap<EventActorImage, EventActorImageDto>();

        CreateMap<TicketType, TicketTypeDto>()
            .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Area != null ? src.Area.Name : null))
            .ForMember(dest => dest.AreaType, opt => opt.MapFrom(src => src.Area != null ? src.Area.Type.ToString() : null));

        CreateMap<EventSession, EventSessionDto>();

        CreateMap<EventHashtag, EventHashtagDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.HashtagId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Hashtag != null ? src.Hashtag.Name : string.Empty));

        CreateMap<EventCategory, EventCategoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
    }
}