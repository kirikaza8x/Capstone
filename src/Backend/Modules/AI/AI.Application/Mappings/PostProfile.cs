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
        // DISTRIBUTION STATUS
        // ========================
        CreateMap<ExternalDistribution, DistributionStatusDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // ========================
        // BASE DTO (Generic list views)
        // ========================
        CreateMap<PostMarketing, PostDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Distributions, opt => opt.MapFrom(src => src.ExternalDistributions));

        // ========================
        // ADMIN LIST ITEM (Moderation queue)
        // ========================
        CreateMap<PostMarketing, PostAdminListItemDto>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Distributions, opt => opt.MapFrom(src => src.ExternalDistributions))
            .ForMember(dest => dest.EventTitle, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizerName, opt => opt.Ignore());

        // ========================
        // DETAIL DTO (Full view with permissions)
        // ========================
        CreateMap<PostMarketing, PostDetailDto>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Distributions, opt => opt.MapFrom(src => src.ExternalDistributions))
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
            .ForMember(dest => dest.TrackingUrl, opt => opt.Ignore());

        // ========================
        // PENDING ITEM (Admin moderation queue)
        // ========================
        CreateMap<PostMarketing, PostPendingItemDto>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.EventTitle, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizerName, opt => opt.Ignore());
    }
}