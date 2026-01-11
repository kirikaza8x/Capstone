using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Commands.Handlers;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.LoginRequest.EmailOrUserName)
            .NotEmpty().WithMessage("Email or username is required.")
            .MaximumLength(100);

        RuleFor(x => x.LoginRequest.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
    }
}

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IValidator<LoginUserCommand> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _validator = validator;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            return Result.Failure<LoginResponseDto>(
                Error.Validation("Login.Validation", firstError.ErrorMessage)
            );
        }

        var user = await _userRepository.GetUserByMailOrUserName(command.LoginRequest.EmailOrUserName, cancellationToken);
        if (user == null)
        {
            return Result.Failure<LoginResponseDto>(
                Error.NotFound("User.NotFound", "User not found.")
            );
        }

        if (!_passwordHasher.VerifyPassword(command.LoginRequest.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponseDto>(
                Error.Unauthorized("User.InvalidCredentials", "Invalid password.")
            );
        }

        var roles = user.Roles.Select(r => r.Name).ToList();
        var accessToken = _jwtTokenService.GenerateToken(user.Id, user.Email, user.UserName, roles);

        string refreshToken;
        DateTime refreshExpiry;

        if (!string.IsNullOrEmpty(user.RefreshToken) &&
            user.RefreshTokenExpiry.HasValue &&
            user.RefreshTokenExpiry > DateTime.UtcNow)
        {
            refreshToken = user.RefreshToken!;
        }
        else
        {
            refreshToken = _jwtTokenService.GenerateRefreshToken();
            refreshExpiry = DateTime.UtcNow.AddDays(_jwtTokenService.RefreshTokenExpiryDays);
            user.SetRefreshToken(refreshToken, refreshExpiry);
            _userRepository.Update(user);
        }

        var response = new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtTokenService.ExpiryMinutes),
            User: new UserInfoDto(user.Id, user.FirstName ?? "", user.UserName, user.Email, roles)
        );

        return Result.Success(response);
    }
}
