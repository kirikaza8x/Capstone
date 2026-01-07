using AutoMapper;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;

namespace Users.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Roles,
                        opt => opt.MapFrom(src => src.Roles.Select(r => r.ToString()).ToList()));

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
                    null                            // role
                ))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
