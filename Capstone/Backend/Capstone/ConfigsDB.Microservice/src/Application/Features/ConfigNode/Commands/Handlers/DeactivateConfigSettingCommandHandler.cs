using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using ConfigsDB.Domain.Repositories;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands.Handlers
{
    // Validator
    public class DeactivateConfigSettingCommandValidator : AbstractValidator<DeactivateConfigSettingCommand>
    {
        public DeactivateConfigSettingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Configuration ID is required.");
        }
    }

    // Handler
    public class DeactivateConfigSettingCommandHandler 
        : ICommandHandler<DeactivateConfigSettingCommand, bool>
    {
        private readonly IConfigSettingRepository _repository;

        public DeactivateConfigSettingCommandHandler(IConfigSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(
            DeactivateConfigSettingCommand command, 
            CancellationToken cancellationToken)
        {
            var config = await _repository.GetByIdAsync(command.Id, cancellationToken);

            if (config == null)
            {
                return Result.Failure<bool>(
                    new Error("ConfigSettingNotFound", $"Configuration with ID '{command.Id}' not found."));
            }

            config.Deactivate();

            await _repository.UpdateAsync(config, cancellationToken);

            return Result.Success(true);
        }
    }
}