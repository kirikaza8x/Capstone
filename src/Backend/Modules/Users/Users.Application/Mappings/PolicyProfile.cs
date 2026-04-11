using AutoMapper;
using Users.Application.Features.Policies.Dtos;
using Users.Domain.Entities;

namespace Users.Application.Mappings;

public class PolicyProfile : Profile
{
    public PolicyProfile()
    {
        CreateMap<Policy, PolicyDto>();
    }
}
