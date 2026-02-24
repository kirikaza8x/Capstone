using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.Handlers;

public class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDeviceDetectionService _deviceDetectionService;
    private readonly IUserUnitOfWork _unitOfWork;

    public GoogleLoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService,
        IDeviceDetectionService deviceDetectionService,
        IUserUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _deviceDetectionService = deviceDetectionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponseDto>> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        // ProviderKey is the Google "sub" claim from the validated ID token
        var providerKey = command.ProviderKey;

        var user = await _userRepository.GetByExternalIdentityAsync("Google", providerKey);
        if (user == null)
        {
            // First-time login → register new user
            user = User.Create(
                command.Email,
                command.UserName ?? Guid.NewGuid().ToString(),
                passwordHash: string.Empty,
                firstName: command.FirstName,
                lastName: command.LastName
            );
            user.BindExternalIdentity("Google", providerKey);

            _userRepository.Add(user);
        }

        var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();

        var deviceId = _currentUserService.DeviceId;
        var deviceInfo = _deviceDetectionService.GetDeviceInfo(
            _currentUserService.UserAgent,
            _currentUserService.IpAddress,
            deviceId);

        if (!string.IsNullOrWhiteSpace(command.DeviceName))
            deviceInfo.DeviceName = command.DeviceName;

        var newToken = _refreshTokenService.GenerateToken(user.Id);

        user.RevokeRefreshTokensByDevice(deviceInfo.DeviceId);

        var refreshTokenEntity = await _userRepository.AddOrUpdateRefreshTokenAsync(
            user,
            RefreshToken.Create(
                newToken.Token,
                newToken.ExpiryDate,
                user.Id,
                deviceInfo.DeviceId,
                deviceInfo.DeviceName,
                deviceInfo.IpAddress,
                deviceInfo.UserAgent
            ),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email,
            user.UserName,
            roles
        );

        var response = new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshTokenEntity.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtTokenService.ExpiryMinutes),
            User: new UserInfoDto(user.Id, user.FirstName ?? "", user.UserName, user.Email, roles),
            DeviceId: deviceInfo.DeviceId,
            DeviceName: deviceInfo.DeviceName
        );

        return Result.Success(response);
    }
}
