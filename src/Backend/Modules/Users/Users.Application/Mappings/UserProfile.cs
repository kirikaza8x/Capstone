using AutoMapper;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;

namespace Users.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Map User → UserResponseDto
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Roles,
                    opt => opt.MapFrom(src => src.Roles.Select(r => r.Name).ToList()));

            // Map RegisterRequestDto → User
            CreateMap<RegisterRequestDto, User>()
                .ConstructUsing(src => User.Create(
                    src.Email,
                    src.UserName!,
                    string.Empty,
                    src.FirstName,
                    src.LastName,
                    src.PhoneNumber,
                    src.Address,
                    null,                        // profileImageUrl
                    null                         // role
                ))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Roles,
                    opt => opt.MapFrom(src => src.Roles.Select(r => r.Name).ToList()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.SocialLink, opt => opt.MapFrom(src => src.SocialLink))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            ;
        }
    }
}
