using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.Handlers;

public class LoginUserCommandHandler
    : ICommandHandler<LoginUserCommand, LoginResponseDto>
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

    // ============================================================
    // Handle
    // ============================================================
    public async Task<Result<LoginResponseDto>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
            return validation;

        var user = await GetUserAsync(command, cancellationToken);
        if (user is null)
            return Result.Failure<LoginResponseDto>(
                Error.NotFound("User.NotFound", "User not found."));

        if (!VerifyPassword(command.Password, user))
            return Result.Failure<LoginResponseDto>(
                Error.Unauthorized("User.InvalidCredentials", "Invalid password."));

        var roles = user.Roles.Select(r => r.Name).ToList();

        var deviceInfo = ResolveDevice(command);

        var refreshToken = await CreateRefreshTokenAsync(
            user,
            deviceInfo,
            cancellationToken);

        var accessToken = CreateAccessToken(user, roles);

        return Result.Success(
            BuildResponse(
                user,
                roles,
                accessToken,
                refreshToken,
                deviceInfo));
    }

    // ============================================================
    // Validation
    // ============================================================
    private async Task<Result<LoginResponseDto>> ValidateAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            var error = result.Errors.First();
            return Result.Failure<LoginResponseDto>(
                Error.Validation("Login.Validation", error.ErrorMessage));
        }

        return Result.Success<LoginResponseDto>(default!);
    }

    // ============================================================
    // User & Credentials
    // ============================================================
    private async Task<User?> GetUserAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserByMailOrUserNameAsync(
            command.EmailOrUserName,
            cancellationToken);
    }

    private bool VerifyPassword(string password, User user)
    {
        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    // ============================================================
    // Device
    // ============================================================
    private DeviceInfo ResolveDevice(LoginUserCommand command)
    {
        var deviceInfo = _deviceDetectionService.GetDeviceInfo(
            _currentUserService.UserAgent,
            _currentUserService.IpAddress,
            _currentUserService.DeviceId);

        if (!string.IsNullOrWhiteSpace(command.DeviceName))
            deviceInfo.DeviceName = command.DeviceName;

        return deviceInfo;
    }

    // ============================================================
    // Tokens
    // ============================================================
    private async Task<RefreshToken> CreateRefreshTokenAsync(
        User user,
        DeviceInfo deviceInfo,
        CancellationToken cancellationToken)
    {
        var generated = _refreshTokenService.GenerateToken(user.Id);

        user.RevokeRefreshTokensByDevice(deviceInfo.DeviceId);

        var refreshToken = RefreshToken.Create(
            generated.Token,
            generated.ExpiryDate,
            user.Id,
            deviceInfo.DeviceId,
            deviceInfo.DeviceName,
            deviceInfo.IpAddress,
            deviceInfo.UserAgent);

        var persisted = await _userRepository.AddOrUpdateRefreshTokenAsync(
            user,
            refreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return persisted;
    }

    private string CreateAccessToken(User user, List<string> roles)
    {
        return _jwtTokenService.GenerateToken(
            user.Id,
            user.Email,
            user.UserName,
            roles);
    }

    // ============================================================
    // Response
    // ============================================================
    private LoginResponseDto BuildResponse(
        User user,
        List<string> roles,
        string accessToken,
        RefreshToken refreshToken,
        DeviceInfo deviceInfo)
    {
        return new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtTokenService.ExpiryMinutes),
            User: new UserInfoDto(
                user.Id,
                user.FirstName ?? string.Empty,
                user.UserName,
                user.Email,
                roles),
            DeviceId: deviceInfo.DeviceId,
            DeviceName: deviceInfo.DeviceName);
    }
}