using AutoMapper;
using Events.Application.Events.Queries.GetEvent;
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
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Hashtag != null ? src.Hashtag.Name : string.Empty));
    }
}