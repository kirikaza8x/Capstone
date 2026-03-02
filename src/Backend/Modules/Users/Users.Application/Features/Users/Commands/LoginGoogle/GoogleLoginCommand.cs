//using Shared.Application.Messaging;
//using Users.Application.Features.Users.Dtos;

//namespace Users.Application.Features.Users.Commands.Records
//{
//    public record GoogleLoginCommand(
//        string ProviderKey,       // Google "sub" claim (unique ID)
//        string? Email,            // optional email from Google profile
//        string? UserName,         // optional username (fallback to Guid if null)
//        string? FirstName,        // optional first name
//        string? LastName,         // optional last name
//        string? DeviceName = null // optional device name override
//    ) : ICommand<LoginResponseDto>;
//}