using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Repositories;
using FluentValidation;
using Users.Domain.UOW;
using Users.Domain.Entities;

namespace Users.Application.Features.Users.Commands.Handlers;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty()
            .WithMessage("Email or username is required.")
            .MaximumLength(256)
            .WithMessage("Email or username must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.DeviceId)
            .MaximumLength(128)
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceId))
            .WithMessage("DeviceId must not exceed 128 characters.");

        RuleFor(x => x.DeviceName)
            .MaximumLength(128)
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceName))
            .WithMessage("DeviceName must not exceed 128 characters.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress))
            .WithMessage("IpAddress must not exceed 64 characters.");

        RuleFor(x => x.UserAgent)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.UserAgent))
            .WithMessage("UserAgent must not exceed 512 characters.");
    }
}

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
        // Validate input
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First();
            return Result.Failure<LoginResponseDto>(
                Error.Validation("Login.Validation", firstError.ErrorMessage)
            );
        }

        // ✅ Load user with roles and refresh tokens tracked
        var user = await _userRepository.GetUserByMailOrUserName(command.EmailOrUserName, cancellationToken);
        if (user == null)
        {
            return Result.Failure<LoginResponseDto>(
                Error.NotFound("User.NotFound", "User not found.")
            );
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponseDto>(
                Error.Unauthorized("User.InvalidCredentials", "Invalid password.")
            );
        }

        var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();

        // Auto-detect device information
        var userAgent = _currentUserService.UserAgent;
        var ipAddress = _currentUserService.IpAddress;
        var deviceInfo = _deviceDetectionService.GetDeviceInfo(userAgent, ipAddress, command.DeviceId);

        if (!string.IsNullOrWhiteSpace(command.DeviceName))
            deviceInfo.DeviceName = command.DeviceName;

        // ✅ Use aggregate root to manage refresh tokens
        var existingToken = user.RefreshTokens
            .FirstOrDefault(rt => rt.DeviceId == deviceInfo.DeviceId && _refreshTokenService.ValidateToken(rt));

        string refreshToken;
        if (existingToken != null)
        {
            refreshToken = existingToken.Token;
            existingToken.UpdateDeviceInfo(deviceInfo.DeviceName, ipAddress, userAgent);
        }
        else
        {
            var newToken = _refreshTokenService.GenerateToken(user.Id);
            var tokenEntity = user.AddRefreshToken(
                newToken.Token,
                newToken.ExpiryDate,
                deviceInfo.DeviceId,
                deviceInfo.DeviceName,
                ipAddress,
                userAgent
            );
            refreshToken = tokenEntity.Token;
        }

        // ✅ No Update(user) call — EF is already tracking
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate access token
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
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtTokenService.ExpiryMinutes),
            User: new UserInfoDto(user.Id, user.FirstName ?? "", user.UserName, user.Email, roles),
            DeviceId: deviceInfo.DeviceId,
            DeviceName: deviceInfo.DeviceName
        );

        return Result.Success(response);
    }
}
