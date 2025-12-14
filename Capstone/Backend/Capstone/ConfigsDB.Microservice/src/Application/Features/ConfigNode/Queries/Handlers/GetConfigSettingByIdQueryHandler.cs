using AutoMapper;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using ConfigsDB.Application.Features.ConfigSettings.Queries.Records;
using ConfigsDB.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;

namespace ConfigsDB.Application.Features.ConfigSettings.Queries
{
    public class GetConfigSettingByIdQueryHandler 
    : IQueryHandler<GetConfigSettingByIdQuery, ConfigSettingResponseDto>
{
    private readonly IConfigSettingRepository _userRepository;
    private readonly IMapper _mapper;

    public GetConfigSettingByIdQueryHandler(IConfigSettingRepository configSettingRepository, IMapper mapper)
    {
        _userRepository = configSettingRepository;
        _mapper = mapper;
    }

    public async Task<Result<ConfigSettingResponseDto>> Handle(GetConfigSettingByIdQuery request, CancellationToken cancellationToken)
    {
        var configSetting = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (configSetting is null)
            return Result.Failure<ConfigSettingResponseDto>(new Error("ConfigSettingNotFound", "ConfigSetting not found"));
        return Result.Success(_mapper.Map<ConfigSettingResponseDto>(configSetting));
    }
}
}
