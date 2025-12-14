using AutoMapper;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using ConfigsDB.Application.Features.ConfigSettings.Queries.Records;
using ConfigsDB.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;

namespace ConfigsDB.Application.Features.ConfigSettings.Queries
{
    public class GetAllConfigSettingsQueryHandler 
        : IQueryHandler<GetAllConfigSettingsQuery, IEnumerable<ConfigSettingResponseDto>>
    {
        private readonly IConfigSettingRepository _repo;
        private readonly IMapper _mapper;

        public GetAllConfigSettingsQueryHandler(IConfigSettingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ConfigSettingResponseDto>>> Handle(GetAllConfigSettingsQuery request, CancellationToken cancellationToken)
        {
            var configSettings = await _repo.GetAllAsync(cancellationToken);
            var dtos = configSettings.Select(_mapper.Map<ConfigSettingResponseDto>);
            return Result.Success(dtos);
        }
    }
}
