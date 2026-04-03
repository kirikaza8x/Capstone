using AI.Application.Features.AiPackages.Dtos;
using AI.Domain.Entities;
using AutoMapper;

namespace AI.Application.Mappings;

public class AiPackageProfile : Profile
{
    public AiPackageProfile()
    {
        CreateMap<AiPackage, AiPackageDto>();
    }
}
