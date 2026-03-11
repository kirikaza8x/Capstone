using AutoMapper;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Entities;

namespace Users.Application.Mappings
{
    public class OrganizersProfile : Profile
    {
        public OrganizersProfile()
        {
            // Map OrganizerProfile -> OrganizerAdminListItemDto
            CreateMap<OrganizerProfile, OrganizerAdminListItemDto>()
                .ForMember(dest => dest.ProfileId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => src.BusinessType))
                .ForMember(dest => dest.VersionNumber, opt => opt.MapFrom(src => src.VersionNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
