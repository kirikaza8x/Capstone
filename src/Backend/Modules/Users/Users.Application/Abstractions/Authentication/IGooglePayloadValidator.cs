using Google.Apis.Auth;

namespace Users.Application.Abstractions.Authentication
{
    public interface IGooglePayloadValidator: IExternalPayloadValidator<GoogleJsonWebSignature.Payload>
    {

    }
}
