using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using Shared.Application.DTOs;
using Users.Application.Features.User.Commands.Login;
using Users.Domain.Repositories;

namespace ClothingStore.Application.Features.User.Commands.Login
{

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

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByMailOrUserName(command.LoginRequest.EmailOrUserName, cancellationToken);
            if (user == null)
            {
                return Result.Failure<LoginResponseDto>(new Error("UserNotFound", "User not found."));
            }

            if (!_passwordHasher.VerifyPassword(command.LoginRequest.Password, user.PasswordHash))
            {
                return Result.Failure<LoginResponseDto>(new Error("InvalidCredentials", "Invalid password."));
            }

            var roles = user.Roles
                    .Select(r => r.Name)   
                    .ToList();
            var accessToken = _jwtTokenService.GenerateToken(user.Id, user.Email, user.UserName, roles);

            string refreshToken;
            DateTime refreshExpiry;

            if (!string.IsNullOrEmpty(user.RefreshToken) && user.RefreshTokenExpiry.HasValue && user.RefreshTokenExpiry > DateTime.UtcNow)
            {
                // Reuse existing valid refresh token
                refreshToken = user.RefreshToken!;
            }
            else
            {
                // Generate new refresh token
                refreshToken = _jwtTokenService.GenerateRefreshToken();
                refreshExpiry = DateTime.UtcNow.AddDays(_jwtTokenService.RefreshTokenExpiryDays);
                user.SetRefreshToken(refreshToken, refreshExpiry);
                _userRepository.Update(user, cancellationToken);
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

}