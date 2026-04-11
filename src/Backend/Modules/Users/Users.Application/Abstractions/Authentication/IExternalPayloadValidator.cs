using Shared.Domain.Abstractions;

namespace Users.Application.Abstractions.Authentication
{
    public interface IExternalPayloadValidator<TPayload>
    {
        abstract Task<Result<TPayload>> ValidateAsync(string idToken);

    }
}
