using AutoMapper;
using Users.Application.Features.Roles.Dtos;
using Users.Domain.Entities;

namespace Users.Application.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<Role, RoleResponseDto>();
            CreateMap<RoleRequestDto, Role>()
                .ConstructUsing(src => Role.Create(
                                            src.Name,
                                            src.Description
                ));
        }
    }
}
