using AutoMapper;
using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Entities;
using Marketing.Domain.Enums;

namespace Marketing.Application.Posts.Mapping;

public class MarketingProfile : Profile
{
    public MarketingProfile()
    {
        // ========================
        // BASE DTO (Generic list views)
        // ========================
        CreateMap<PostMarketing, PostDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            // Removed Platform mapping - property doesn't exist on Post entity

        // ========================
        // ADMIN LIST ITEM (Moderation queue)
        // ========================
        CreateMap<PostMarketing, PostAdminListItemDto>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            // These fields are populated via Events.PublicApi / Users.PublicApi in handler
            .ForMember(dest => dest.EventTitle, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizerName, opt => opt.Ignore());

        // ========================
        // DETAIL DTO (Full view with permissions)
        // ========================
        CreateMap<PostMarketing, PostDetailDto>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            // Computed permissions - use traditional comparisons (not pattern matching)
            .ForMember(dest => dest.CanEdit, opt => opt.MapFrom(src =>
                src.Status == PostStatus.Draft || src.Status == PostStatus.Rejected))
            .ForMember(dest => dest.CanSubmit, opt => opt.MapFrom(src =>
                src.Status == PostStatus.Draft || src.Status == PostStatus.Rejected))
            .ForMember(dest => dest.CanPublish, opt => opt.MapFrom(src =>
                src.Status == PostStatus.Approved))
            .ForMember(dest => dest.CanArchive, opt => opt.MapFrom(src =>
                src.Status != PostStatus.Archived && src.Status != PostStatus.Pending));

        // ========================
        // PUBLIC DTO (Attendee-facing view)
        // ========================
        CreateMap<PostMarketing, PostPublicDto>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            // TrackingUrl is built in handler with baseUrl + eventCode
            .ForMember(dest => dest.TrackingUrl, opt => opt.Ignore());

        // ========================
        // PENDING ITEM (Admin moderation queue)
        // ========================
        CreateMap<PostMarketing, PostPendingItemDto>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            // These fields are populated via Events.PublicApi / Users.PublicApi in handler
            .ForMember(dest => dest.EventTitle, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizerName, opt => opt.Ignore());
    }
}