using Shared.Application.Abstractions.Authentication;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Abstractions.Authentication;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.Commands.LoginGoogle;

public class GoogleLoginCommandHandler(
    IUserRepository userRepository,
    IGooglePayloadValidator googleValidator,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    ICurrentUserService currentUserService,
    IDeviceDetectionService deviceDetectionService,
    IUserUnitOfWork unitOfWork) : ICommandHandler<GoogleLoginCommand, LoginResponseDto>
{
    public async Task<Result<LoginResponseDto>> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate Google Token
        var payloadResult = await googleValidator.ValidateAsync(command.IdToken);
        if (payloadResult.IsFailure) return Result.Failure<LoginResponseDto>(payloadResult.Error);

        var payload = payloadResult.Value;

        // 2. Find or Register User
        var user = await userRepository.GetByExternalIdentityAsync("Google", payload.Subject);
        if (user == null)
        {
            user = User.CreateExternal(
                payload.Email,
                payload.Email?.Split('@')[0] ?? Guid.NewGuid().ToString(),
                "Google",
                payload.Subject,
                payload.GivenName,
                payload.FamilyName
            );
            userRepository.Add(user);
        }

        var deviceInfo = deviceDetectionService.GetDeviceInfo(
            currentUserService.UserAgent,
            currentUserService.IpAddress,
            currentUserService.DeviceId);

        if (!string.IsNullOrWhiteSpace(command.DeviceName))
            deviceInfo.DeviceName = command.DeviceName;

        var tokenData = refreshTokenService.GenerateToken(user.Id);

        user.LoginDevice(
            tokenData.Token,
            tokenData.ExpiryDate,
            deviceInfo.DeviceId,
            deviceInfo.DeviceName,
            deviceInfo.IpAddress,
            deviceInfo.UserAgent);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Generate Access Token
        var roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>();
        var accessToken = jwtTokenService.GenerateToken(user.Id, user.Email, user.UserName, roles);

        return Result.Success(new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: tokenData.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(jwtTokenService.ExpiryMinutes),
            User: new UserInfoDto(user.Id, user.FirstName ?? "", user.UserName, user.Email, roles),
            DeviceId: deviceInfo.DeviceId,
            DeviceName: deviceInfo.DeviceName
        ));
    }
}