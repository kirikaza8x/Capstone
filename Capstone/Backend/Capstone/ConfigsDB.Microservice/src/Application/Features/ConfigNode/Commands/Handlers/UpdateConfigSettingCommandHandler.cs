using AutoMapper;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;
using ConfigsDB.Domain.Repositories;
using ConfigsDB.Application.Abstractions.Configs; 

namespace ConfigsDB.Application.Features.ConfigSettings.Commands.Handlers
{
    public class UpdateConfigSettingCommandValidator : AbstractValidator<UpdateConfigSettingCommand>
    {
        public UpdateConfigSettingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Configuration ID is required.");

            RuleFor(x => x.Request.Value)
                .NotEmpty().WithMessage("Configuration value is required.")
                .MaximumLength(2000).WithMessage("Value cannot exceed 2000 characters.");

            RuleFor(x => x.Request.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Request.Description));
        }
    }

    public class UpdateConfigSettingCommandHandler 
        : ICommandHandler<UpdateConfigSettingCommand, ConfigSettingResponseDto>
    {
        private readonly IConfigSettingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IConfigDistributor _distributor; 

        public UpdateConfigSettingCommandHandler(
            IConfigSettingRepository repository, 
            IMapper mapper, 
            IConfigDistributor distributor)
        {
            _repository = repository;
            _mapper = mapper;
            _distributor = distributor;
        }

        public async Task<Result<ConfigSettingResponseDto>> Handle(
            UpdateConfigSettingCommand command, 
            CancellationToken cancellationToken)
        {
            var config = await _repository.GetByIdAsync(command.Id, cancellationToken);

            if (config == null)
            {
                return Result.Failure<ConfigSettingResponseDto>(
                    new Error("ConfigSettingNotFound", $"Configuration with ID '{command.Id}' not found."));
            }

            config.UpdateValue(command.Request.Value);

            if (command.Request.Description != null)
            {
                config.UpdateDescription(command.Request.Description);
            }

            config.SetActive(command.Request.IsActive);

            await _repository.UpdateAsync(config, cancellationToken);

            // 3. BROADCAST CHANGES (The "Map Way")
            // The distributor will find the right Strategy (like JwtSyncStrategy) 
            // and publish the integration event automatically.
            await _distributor.DistributeAsync(config.Key, config.Environment, cancellationToken);

            var response = _mapper.Map<ConfigSettingResponseDto>(config);
            return Result.Success(response);
        }
    }
}