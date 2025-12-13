using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using ConfigsDB.Domain.Repositories;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands.Handlers
{
    // Validator
    public class MarkConfigAsPlainTextCommandValidator : AbstractValidator<MarkConfigAsPlainTextCommand>
    {
        public MarkConfigAsPlainTextCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Configuration ID is required.");
        }
    }

    // Handler
    public class MarkConfigAsPlainTextCommandHandler 
        : ICommandHandler<MarkConfigAsPlainTextCommand, bool>
    {
        private readonly IConfigSettingRepository _repository;

        public MarkConfigAsPlainTextCommandHandler(IConfigSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(
            MarkConfigAsPlainTextCommand command, 
            CancellationToken cancellationToken)
        {
            var config = await _repository.GetByIdAsync(command.Id, cancellationToken);

            if (config == null)
            {
                return Result.Failure<bool>(
                    new Error("ConfigSettingNotFound", $"Configuration with ID '{command.Id}' not found."));
            }

            config.MarkAsPlainText();

            await _repository.UpdateAsync(config, cancellationToken);

            return Result.Success(true);
        }
    }
}