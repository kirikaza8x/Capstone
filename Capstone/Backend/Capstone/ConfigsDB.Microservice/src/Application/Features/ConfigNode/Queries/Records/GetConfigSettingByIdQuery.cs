using Shared.Application.Abstractions.Messaging;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
namespace ConfigsDB.Application.Features.ConfigSettings.Queries.Records
{
public record GetConfigSettingByIdQuery(Guid Id) : IQuery<ConfigSettingResponseDto>;
}
