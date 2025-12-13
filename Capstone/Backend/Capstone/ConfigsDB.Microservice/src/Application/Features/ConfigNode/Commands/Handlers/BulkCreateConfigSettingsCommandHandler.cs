using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using ConfigsDB.Domain.Entities;
using ConfigsDB.Domain.Repositories;
using Shared.Domain.Repositories;
using Shared.Domain.Common.Options; // for BulkInsertOptions

namespace ConfigsDB.Application.Features.ConfigSettings.Commands.Handlers
{
    // Validator
    public class BulkCreateConfigSettingsCommandValidator : AbstractValidator<BulkCreateConfigSettingsCommand>
    {
        public BulkCreateConfigSettingsCommandValidator()
        {
            RuleFor(x => x.Request.ConfigSettings)
                .NotEmpty().WithMessage("At least one configuration is required.")
                .Must(list => list.Count <= 1000).WithMessage("Cannot create more than 1000 configurations at once.");

            // Optional: validate each DTO if you have a ConfigSettingRequestDtoValidator
            // RuleForEach(x => x.Request.ConfigSettings)
            //     .SetValidator(new ConfigSettingRequestDtoValidator());
        }
    }

    // Handler
    public class BulkCreateConfigSettingsCommandHandler 
        : ICommandHandler<BulkCreateConfigSettingsCommand, List<ConfigSettingResponseDto>>
    {
        private readonly IConfigSettingRepository _repository;
        private readonly IConfigSettingBulkRepository _bulkRepository;
        private readonly IMapper _mapper;

        public BulkCreateConfigSettingsCommandHandler(
            IConfigSettingRepository repository,
            IConfigSettingBulkRepository bulkRepository,
            IMapper mapper)
        {
            _repository = repository;
            _bulkRepository = bulkRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ConfigSettingResponseDto>>> Handle(
            BulkCreateConfigSettingsCommand command, 
            CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            // Collect distinct keys to check
            var keysToCheck = command.Request.ConfigSettings
                .Select(r => (r.Key, r.Environment))
                .Distinct()
                .ToList();

            // Ask repository for existing keys in one batch query
            var existingKeys = await _repository.GetExistingKeysAsync(keysToCheck, cancellationToken);

            // Build new configs in one pass
            var createdConfigs = command.Request.ConfigSettings
                .Where(r =>
                {
                    if (existingKeys.Contains((r.Key, r.Environment)))
                    {
                        errors.Add($"Configuration '{r.Key}' already exists in environment '{r.Environment}'.");
                        return false;
                    }
                    return true;
                })
                .Select(r =>
                {
                    var config = ConfigSetting.Create(
                        r.Key,
                        r.Value,
                        r.Category,
                        r.Environment,
                        r.Description,
                        r.IsEncrypted
                    );
                    if (!r.IsActive) config.Deactivate();
                    return config;
                })
                .ToList();

            if (createdConfigs.Count == 0)
            {
                return Result.Failure<List<ConfigSettingResponseDto>>(
                    new Error("BulkCreateFailed", $"No configurations were created. Errors: {string.Join("; ", errors)}"));
            }

            // Bulk insert using indirect BulkInsertOptions
            await _bulkRepository.BulkInsertAsync(
                createdConfigs,
                new BulkInsertOptions
                {
                    BatchSize = 500,
                    PreserveInsertOrder = true,
                    SetOutputIdentity = true
                },
                cancellationToken
            );

            var response = _mapper.Map<List<ConfigSettingResponseDto>>(createdConfigs);

            return Result.Success(response);
        }
    }
}
