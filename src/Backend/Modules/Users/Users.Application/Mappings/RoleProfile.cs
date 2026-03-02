using AutoMapper;
using Shared.Application.Dtos.Queries;
using Shared.Domain.Queries;
using Users.Application.Features.Roles.Dtos;
using Users.Application.Features.Roles.Queries;
using Users.Domain.Entities;

namespace Users.Application.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<SortRequestDto, Sort>();
            CreateMap<FilterRequestDto, Filter>()
                .ForMember(dest => dest.Filters, opt => opt.MapFrom(src => src.Filters));

            CreateMap<RoleFilterRequestDto, GetRolesQuery>();

            CreateMap<Role, RoleResponseDto>();

            CreateMap<RoleRequestDto, Role>()
                .ConstructUsing(src => Role.Create(
                    src.Name,
                    src.Description
                ));
        }
    }
}
