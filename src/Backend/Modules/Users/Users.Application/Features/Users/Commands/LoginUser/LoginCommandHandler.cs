using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Repositories;
using FluentValidation;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.Handlers;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDeviceDetectionService _deviceDetectionService;
    private readonly IUserUnitOfWork _unitOfWork;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService,
        IDeviceDetectionService deviceDetectionService,
        IUserUnitOfWork unitOfWork,
        IValidator<LoginUserCommand> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _deviceDetectionService = deviceDetectionService;
        _unitOfWork = unitOfWork;
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

        var user = await _userRepository.GetUserByMailOrUserNameAsync(command.EmailOrUserName, cancellationToken);
        if (user == null)
            return Result.Failure<LoginResponseDto>(Error.NotFound("User.NotFound", "User not found."));

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            return Result.Failure<LoginResponseDto>(Error.Unauthorized("User.InvalidCredentials", "Invalid password."));

        var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();

        var deviceInfo = _deviceDetectionService.GetDeviceInfo(
            _currentUserService.UserAgent,
            _currentUserService.IpAddress,
            command.DeviceId);

        if (!string.IsNullOrWhiteSpace(command.DeviceName))
            deviceInfo.DeviceName = command.DeviceName;

        // Generate a new refresh token
        var newToken = _refreshTokenService.GenerateToken(user.Id);

        // revoke any existing token for this device before adding the new one
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
