using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using ConfigsDB.Domain.Repositories;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands.Handlers
{
    // Validator
    public class DeleteConfigSettingCommandValidator : AbstractValidator<DeleteConfigSettingCommand>
    {
        public DeleteConfigSettingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Configuration ID is required.");
        }
    }

    // Handler
    public class DeleteConfigSettingCommandHandler 
        : ICommandHandler<DeleteConfigSettingCommand, bool>
    {
        private readonly IConfigSettingRepository _repository;

        public DeleteConfigSettingCommandHandler(IConfigSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(
            DeleteConfigSettingCommand command, 
            CancellationToken cancellationToken)
        {
            var config = await _repository.GetByIdAsync(command.Id, cancellationToken);

            if (config == null)
            {
                return Result.Failure<bool>(
                    new Error("ConfigSettingNotFound", $"Configuration with ID '{command.Id}' not found."));
            }

            // Mark as deleted (IsDeleted flag set by interceptor on next save)
            config.Delete("System"); // Triggers soft delete

            await _repository.UpdateAsync(config, cancellationToken);

            return Result.Success(true);
        }
    }
}