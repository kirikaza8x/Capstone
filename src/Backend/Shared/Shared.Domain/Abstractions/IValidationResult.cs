namespace Shared.Domain.Abstractions;

public interface IValidationResult
{
    Error[] Errors { get; }
}
