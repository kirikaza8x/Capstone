using System.Security.Claims;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Login;
using Users.Domain.Repositories;

namespace ClothingStore.Application.Features.Users.Commands.RegisterUser
{
    public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var principal = _jwtTokenService.ValidateToken(request.AccessToken, allowExpired: true);
            if (principal == null)
            {
                return Result.Failure<LoginResponseDto>(
                    Error.Unauthorized("Token.InvalidAccess", "Access token is invalid.")
                );
            }

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<LoginResponseDto>(
                    Error.NotFound("User.NotFound", "User not found.")
                );
            }

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return Result.Failure<LoginResponseDto>(
                    Error.Unauthorized("Token.RefreshExpired", "Refresh token is invalid or expired.")
                );
            }

            var roles = user.Roles.Select(r => r.Name).ToList();
            var newAccessToken = _jwtTokenService.GenerateToken(user.Id, user.Email, user.UserName, roles);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var newExpiry = DateTime.UtcNow.AddDays(_jwtTokenService.RefreshTokenExpiryDays);

            user.SetRefreshToken(newRefreshToken, newExpiry);
            _userRepository.Update(user);

            var response = new LoginResponseDto(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtTokenService.ExpiryMinutes),
                User: new UserInfoDto(user.Id, user.FirstName ?? "", user.UserName, user.Email, roles)
            );

            return Result.Success(response);
        }
    }
}
