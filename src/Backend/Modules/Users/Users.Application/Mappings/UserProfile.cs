using AutoMapper;
using Shared.Application.Dtos.Queries; 
using Shared.Domain.Queries;           
using Users.Application.Features.Users.Dtos;
using Users.Application.Features.Users.Queries; 
using Users.Domain.Entities;

namespace Users.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // --- Shared Query Mappings ---
            // These handle the recursion and base types for all search queries
            CreateMap<SortRequestDto, Sort>();
            
            CreateMap<FilterRequestDto, Filter>()
                .ForMember(dest => dest.Filters, opt => opt.MapFrom(src => src.Filters));

            // Map the Specific User Request to the GetUsersQuery
            // This inherits the PageNumber, PageSize, etc., from the base mappings
            CreateMap<UserFilterRequestDto, GetUsersQuery>();

            // --- User Entity Mappings ---
            
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
                    null,
                    null
                ))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            // User Profile Mapping (Simplified: AutoMapper maps identical names automatically)
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Roles,
                    opt => opt.MapFrom(src => src.Roles.Select(r => r.Name).ToList()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
        }
    }
}