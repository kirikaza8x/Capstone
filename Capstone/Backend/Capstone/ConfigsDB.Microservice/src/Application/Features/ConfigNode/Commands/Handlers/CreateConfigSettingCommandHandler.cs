using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using ConfigsDB.Application.Features.ConfigSettings.Commands;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using ConfigsDB.Domain.Entities;
using ConfigsDB.Domain.Repositories;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands.Handlers
{
    // Validator
    public class CreateConfigSettingCommandValidator : AbstractValidator<CreateConfigSettingCommand>
    {
        public CreateConfigSettingCommandValidator()
        {
            RuleFor(x => x.Request.Key)
                .NotEmpty().WithMessage("Configuration key is required.")
                .MaximumLength(200).WithMessage("Key cannot exceed 200 characters.");

            RuleFor(x => x.Request.Value)
                .NotEmpty().WithMessage("Configuration value is required.")
                .MaximumLength(2000).WithMessage("Value cannot exceed 2000 characters.");

            RuleFor(x => x.Request.Category)
                .NotEmpty().WithMessage("Category is required.")
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters.");

            RuleFor(x => x.Request.Environment)
                .NotEmpty().WithMessage("Environment is required.")
                .MaximumLength(50).WithMessage("Environment cannot exceed 50 characters.")
                .Must(env => new[] { "Global", "Dev", "Development", "Staging", "Prod", "Production", "Test" }
                    .Contains(env, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Invalid environment. Valid values: Global, Dev, Development, Staging, Prod, Production, Test");

            RuleFor(x => x.Request.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Request.Description));
        }
    }

    // Handler
    public class CreateConfigSettingCommandHandler 
        : ICommandHandler<CreateConfigSettingCommand, ConfigSettingResponseDto>
    {
        private readonly IConfigSettingRepository _repository;
        private readonly IMapper _mapper;

        public CreateConfigSettingCommandHandler(IConfigSettingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ConfigSettingResponseDto>> Handle(
            CreateConfigSettingCommand command, 
            CancellationToken cancellationToken)
        {
            // Check if config already exists with same key and environment
            var exists = await _repository.ExistsAsync(
                command.Request.Key,
                command.Request.Environment,
                cancellationToken);

            if (exists)
            {
                return Result.Failure<ConfigSettingResponseDto>(
                    new Error(
                        "ConfigSettingAlreadyExists",
                        $"Configuration '{command.Request.Key}' already exists in environment '{command.Request.Environment}'."));
            }

            // Create entity using factory method (audit fields handled by interceptor)
            var config = ConfigSetting.Create(
                command.Request.Key,
                command.Request.Value,
                command.Request.Category,
                command.Request.Environment,
                command.Request.Description,
                command.Request.IsEncrypted
            );

            // If not active by default, deactivate
            if (!command.Request.IsActive)
            {
                config.Deactivate();
            }

            // Add to repository (SaveChanges handled by UnitOfWorkBehavior)
            await _repository.AddAsync(config, cancellationToken);

            // Map to response DTO
            var response = _mapper.Map<ConfigSettingResponseDto>(config);

            return Result.Success(response);
        }
    }
}