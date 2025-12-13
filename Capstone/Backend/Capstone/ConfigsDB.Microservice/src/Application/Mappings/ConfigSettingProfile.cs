using AutoMapper;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using ConfigsDB.Domain.Entities;

namespace ConfigsDB.Application.Mappings
{
    public class ConfigSettingMappingProfile : Profile
    {
        public ConfigSettingMappingProfile()
        {
            // Entity to Response DTO
            CreateMap<ConfigSetting, ConfigSettingResponseDto>();
            
            CreateMap<ConfigSetting, ConfigSettingSimpleResponseDto>();

            // Request DTO to Entity (handled in Create method)
            // Not directly mapped since we use ConfigSetting.Create() factory method

            // For resolved configs
            CreateMap<ConfigSetting, ResolvedConfigValueDto>()
                .ForMember(dest => dest.ResolvedFrom, opt => opt.MapFrom(src => src.Environment));
        }
    }
}