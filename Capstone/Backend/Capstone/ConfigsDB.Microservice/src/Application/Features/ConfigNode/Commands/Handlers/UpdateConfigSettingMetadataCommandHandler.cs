using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using ConfigsDB.Domain.Repositories;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands.Handlers
{
    // Validator
    public class UpdateConfigSettingMetadataCommandValidator : AbstractValidator<UpdateConfigSettingMetadataCommand>
    {
        public UpdateConfigSettingMetadataCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Configuration ID is required.");

            RuleFor(x => x.Request.Category)
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Request.Category));

            RuleFor(x => x.Request.Environment)
                .MaximumLength(50).WithMessage("Environment cannot exceed 50 characters.")
                .Must(env => new[] { "Global", "Dev", "Development", "Staging", "Prod", "Production", "Test" }
                    .Contains(env, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Invalid environment. Valid values: Global, Dev, Development, Staging, Prod, Production, Test")
                .When(x => !string.IsNullOrEmpty(x.Request.Environment));

            RuleFor(x => x.Request)
                .Must(x => !string.IsNullOrEmpty(x.Category) || !string.IsNullOrEmpty(x.Environment))
                .WithMessage("At least one of Category or Environment must be provided.");
        }
    }

    // Handler
    public class UpdateConfigSettingMetadataCommandHandler 
        : ICommandHandler<UpdateConfigSettingMetadataCommand, ConfigSettingResponseDto>
    {
        private readonly IConfigSettingRepository _repository;
        private readonly IMapper _mapper;

        public UpdateConfigSettingMetadataCommandHandler(IConfigSettingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ConfigSettingResponseDto>> Handle(
            UpdateConfigSettingMetadataCommand command, 
            CancellationToken cancellationToken)
        {
            var config = await _repository.GetByIdAsync(command.Id, cancellationToken);

            if (config == null)
            {
                return Result.Failure<ConfigSettingResponseDto>(
                    new Error("ConfigSettingNotFound", $"Configuration with ID '{command.Id}' not found."));
            }

            // Update category if provided
            if (!string.IsNullOrEmpty(command.Request.Category))
            {
                config.ChangeCategory(command.Request.Category);
            }

            // Update environment if provided
            if (!string.IsNullOrEmpty(command.Request.Environment))
            {
                // Check if changing environment would create a duplicate
                var wouldConflict = await _repository.ExistsAsync(
                    config.Key,
                    command.Request.Environment,
                    cancellationToken);

                if (wouldConflict)
                {
                    return Result.Failure<ConfigSettingResponseDto>(
                        new Error(
                            "ConfigSettingConflict",
                            $"Configuration '{config.Key}' already exists in environment '{command.Request.Environment}'."));
                }

                config.ChangeEnvironment(command.Request.Environment);
            }

            await _repository.UpdateAsync(config, cancellationToken);

            var response = _mapper.Map<ConfigSettingResponseDto>(config);
            return Result.Success(response);
        }
    }
}