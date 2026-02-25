using Google.Apis.Auth;
using Shared.Domain.Abstractions;
using Shared.Infrastructure.Configs.Security;
using Microsoft.Extensions.Options;
using Users.Application.Abstractions.Authentication;

public class GooglePayloadValidatorService : IGooglePayloadValidator
{
    private readonly GoogleAuthConfigs _configs;

    public GooglePayloadValidatorService(IOptions<GoogleAuthConfigs> configs)
    {
        _configs = configs.Value;
    }

    public async Task<Result<GoogleJsonWebSignature.Payload>> ValidateAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configs.ServerClientId }
                });

            return Result.Success(payload);
        }
        catch (InvalidJwtException ex)
        {
            return Result.Failure<GoogleJsonWebSignature.Payload>(
                Error.Validation("InvalidGoogleToken", $"Google token validation failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<GoogleJsonWebSignature.Payload>(
                Error.Failure("GoogleValidationError", $"Unexpected error during token validation: {ex.Message}"));
        }
    }
}
