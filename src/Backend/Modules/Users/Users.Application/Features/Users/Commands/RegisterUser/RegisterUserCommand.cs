using Shared.Application.Messaging;
using Users.Application.Features.Users.Dtos;
using Users.Application.Messaging;

namespace Users.Application.Features.Users.Commands.Records;

// public record RegisterUserCommand(
//     string Email,
//     string UserName,
//     string Password
// ) : IRequest<UserResponse>;

public record RegisterUserCommand( 
    string Email, 
    string UserName, 
    string Password, 
    string FirstName, 
    string LastName, 
    string PhoneNumber, 
    string Address ) : ICommand<UserResponseDto>,IUserSaveCommand;