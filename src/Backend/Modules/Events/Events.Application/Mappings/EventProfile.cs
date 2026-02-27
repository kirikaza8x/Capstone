using AutoMapper;
using Events.Application.Events.Queries.GetEventById;
using Events.Application.Events.Queries.GetEvents;
using Events.Domain.Entities;

namespace Events.Application.Mappings;

public sealed class EventProfile : Profile
{
    public EventProfile()
    {
        // Event mappings
        CreateMap<Event, GetEventResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Hashtags, opt => opt.MapFrom(src => src.EventHashtags));

        CreateMap<EventImage, EventImageDto>();
        CreateMap<EventSession, EventSessionDto>();
        CreateMap<EventHashtag, EventHashtagDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.HashtagId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Hashtag != null ? src.Hashtag.Name : string.Empty));

        CreateMap<Event, EventResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}