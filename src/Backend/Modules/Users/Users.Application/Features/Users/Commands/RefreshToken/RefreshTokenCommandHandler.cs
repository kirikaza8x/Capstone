using System.Security.Claims;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Handlers.RefreshTokenCommandHandler
{
    public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDeviceDetectionService _deviceDetectionService;
        private readonly IValidator<RefreshTokenCommand> _validator;
        private readonly IUserUnitOfWork _unitOfWork; 

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            ICurrentUserService currentUserService,
            IDeviceDetectionService deviceDetectionService,
            IValidator<RefreshTokenCommand> validator,
            IUserUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _currentUserService = currentUserService;
            _deviceDetectionService = deviceDetectionService;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.First();
                return Result.Failure<LoginResponseDto>(
                    Error.Validation("RefreshToken.Validation", firstError.ErrorMessage)
                );
            }

            var principal = _jwtTokenService.ValidateToken(command.AccessToken, allowExpired: true);
            if (principal == null)
            {
                return Result.Failure<LoginResponseDto>(
                    Error.Unauthorized("Token.InvalidAccess", "Access token is invalid.")
                );
            }

            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<LoginResponseDto>(
                    Error.NotFound("User.NotFound", "User not found.")
                );
            }

            var storedToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.Token == command.RefreshToken);

            if (storedToken == null || !_refreshTokenService.ValidateToken(storedToken))
            {
                return Result.Failure<LoginResponseDto>(
                    Error.Unauthorized("Token.RefreshExpired", "Refresh token is invalid or expired.")
                );
            }

            // Revoke old token and issue new one
            _refreshTokenService.RevokeToken(storedToken);
            var newRefreshToken = _refreshTokenService.GenerateToken(user.Id);

            var userAgent = _currentUserService.UserAgent ?? command.UserAgent;
            var ipAddress = _currentUserService.IpAddress ?? command.IpAddress;
            var deviceInfo = _deviceDetectionService.GetDeviceInfo(userAgent, ipAddress, command.DeviceId);

            user.AddRefreshToken(
                newRefreshToken.Token,
                newRefreshToken.ExpiryDate,
                deviceInfo.DeviceId,
                command.DeviceName ?? deviceInfo.DeviceName,
                ipAddress,
                userAgent
            );

            var roles = user.Roles.Select(r => r.Name).ToList();

            var newAccessToken = _jwtTokenService.GenerateToken(
                user.Id,
                user.Email,
                user.UserName,
                roles,
                deviceInfo.DeviceId,
                deviceInfo.IpAddress,
                deviceInfo.UserAgent,
                deviceInfo.DeviceName,
                deviceInfo.Browser,
                deviceInfo.OperatingSystem,
                deviceInfo.DeviceType,
                deviceInfo.BrowserVersion,
                deviceInfo.OSVersion
            );

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken); 
            var response = new LoginResponseDto(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken.Token,
                ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtTokenService.ExpiryMinutes),
                User: new UserInfoDto(user.Id, user.FirstName ?? "", user.UserName, user.Email, roles),
                DeviceId: deviceInfo.DeviceId,
                DeviceName: command.DeviceName ?? deviceInfo.DeviceName
            );

            return Result.Success(response);
        }
    }
}
