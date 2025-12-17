namespace ConfigsDB.Infrastructure.Consumers;

using MassTransit;
using Shared.Contracts.Events.Configs;
using ConfigsDB.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetJwtConfigurationConsumer : IConsumer<IGetJwtConfigurationRequest>
{
    private readonly IConfigSettingRepository _repository;
    private readonly ILogger<GetJwtConfigurationConsumer> _logger;

    public GetJwtConfigurationConsumer(IConfigSettingRepository repository, ILogger<GetJwtConfigurationConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGetJwtConfigurationRequest> context)
    {
        var expirySetting = await _repository.GetByKeyAndEnvironmentAsync("Jwt.ExpiryMinutes", "Global");
        var refreshSetting = await _repository.GetByKeyAndEnvironmentAsync("Jwt.RefreshDays", "Global");

        int expiry = int.TryParse(expirySetting?.Value, out var e) ? e : 60;
        int refresh = int.TryParse(refreshSetting?.Value, out var r) ? r : 7;

        _logger.LogInformation("Responding to JWT config request: {Exp}m, {Ref}d", expiry, refresh);

        await context.RespondAsync<IJwtConfigurationResponse>(new
        {
            context.Message.CorrelationId,
            SourceService = "ConfigsDB",
            OccurredAt = DateTime.UtcNow,
            Version = 1,
            ExpiryMinutes = expiry,
            RefreshTokenExpiryDays = refresh
        });
    }
}