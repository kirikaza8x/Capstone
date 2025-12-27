// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Google.Apis.Auth;
// using Microsoft.Extensions.Options;
// using FluentValidation;
// using ClothingStore.Domain.Enums;
// using ClothingStore.Domain.Entities;
// using ClothingStore.Domain.Repositories;
// using Shared.Application.Abstractions.Messaging;
// using Shared.Domain.Common.ResponseModel;
// using Shared.Application.Abstractions.DTOs;
// using Shared.Application.Abstractions.Authentication;
// using Shared.Infrastructure.Configs.Security;

// namespace ClothingStore.Application.Features.Auth.Commands.GoogleLogin
// {
    
//     // ============================================
//     // Validator
//     // ============================================
//     public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
//     {
//         public GoogleLoginCommandValidator()
//         {
//             RuleFor(x => x.IdToken)
//                 .NotEmpty()
//                 .WithMessage("Google ID token is required.")
//                 .MinimumLength(100)
//                 .WithMessage("Invalid Google ID token format.")
//                 .Must(token => !token.Contains(" "))
//                 .WithMessage("Google ID token cannot contain spaces.");
//         }
//     }

//     // ============================================
//     // Handler
//     // ============================================
//     public class GoogleLoginCommandHandler
//         : ICommandHandler<GoogleLoginCommand, LoginResponseDto>
//     {
//         private const int MaxUsernameLength = 30;
//         private const int GeneratedUsernameLength = 12;

//         private readonly IUserRepository _userRepository;
//         private readonly IExternalIdentityRepository _externalRepo;
//         private readonly IJwtTokenService _jwtTokenService;
//         private readonly GoogleAuthConfigs _googleAuthConfigs;

//         public GoogleLoginCommandHandler(
//             IUserRepository userRepository,
//             IExternalIdentityRepository externalRepo,
//             IJwtTokenService jwtTokenService,
//             IOptions<GoogleAuthConfigs> googleAuthConfigs)
//         {
//             _userRepository = userRepository;
//             _externalRepo = externalRepo;
//             _jwtTokenService = jwtTokenService;
//             _googleAuthConfigs = googleAuthConfigs.Value;
//         }

//         public async Task<Result<LoginResponseDto>> Handle(
//             GoogleLoginCommand command,
//             CancellationToken cancellationToken)
//         {
//             // Validate Google token
//             var audience = _googleAuthConfigs.ServerClientId;
//             var payloadResult = await ValidateGoogleTokenAsync(command.IdToken, audience);
//             if (payloadResult.IsFailure)
//                 return Result.Failure<LoginResponseDto>(payloadResult.Error);

//             var payload = payloadResult.Value;

//             // Get or create user
//             var userResult = await GetOrCreateUserAsync(payload, cancellationToken);
//             if (userResult.IsFailure)
//                 return Result.Failure<LoginResponseDto>(userResult.Error);

//             var (user, isNewUser, needsUpdate) = userResult.Value;

//             // Generate tokens
//             var (refreshToken, refreshExpiry, tokenNeedsUpdate) =
//                 GenerateOrReuseRefreshToken(user);

//             if (tokenNeedsUpdate)
//             {
//                 user.SetRefreshToken(refreshToken, refreshExpiry);
//                 needsUpdate = true;
//             }

//             // Only update if user exists and needs update
//             // New users are already added via AddAsync
//             if (!isNewUser && needsUpdate)
//             {
//                 _userRepository.Update(user, cancellationToken);
//             }

//             // Build response
//             var roles = user.Roles.Select(r => r.ToString()).ToArray();
//             var accessToken = _jwtTokenService.GenerateToken(
//                 user.Id, user.Email, user.UserName, roles);

//             var response = new LoginResponseDto(
//                 AccessToken: accessToken,
//                 RefreshToken: refreshToken,
//                 ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtTokenService.ExpiryMinutes),
//                 User: new UserInfoDto(
//                     user.Id,
//                     user.FirstName ?? string.Empty,
//                     user.UserName,
//                     user.Email,
//                     roles)
//             );

//             return Result.Success(response);
//         }

//         private static async Task<Result<GoogleJsonWebSignature.Payload>> ValidateGoogleTokenAsync(
//             string idToken,
//             string audience)
//         {
//             try
//             {
//                 var payload = await GoogleJsonWebSignature.ValidateAsync(
//                     idToken,
//                     new GoogleJsonWebSignature.ValidationSettings
//                     {
//                         Audience = new[] { audience }
//                     });

//                 return Result.Success(payload);
//             }
//             catch (InvalidJwtException ex)
//             {
//                 return Result.Failure<GoogleJsonWebSignature.Payload>(
//                     new Error("InvalidGoogleToken", $"Google token validation failed: {ex.Message}"));
//             }
//             catch (Exception ex)
//             {
//                 return Result.Failure<GoogleJsonWebSignature.Payload>(
//                     new Error("GoogleValidationError", $"Unexpected error during token validation: {ex.Message}"));
//             }
//         }

//         private async Task<Result<(Users User, bool IsNewUser, bool NeedsUpdate)>> GetOrCreateUserAsync(
//             GoogleJsonWebSignature.Payload payload,
//             CancellationToken ct)
//         {
//             var needsUpdate = false;
//             var isNewUser = false;

//             // Try to find existing external identity
//             var identity = await _externalRepo.FindExternalAsync(
//                 AuthProvider.Google,
//                 payload.Subject,
//                 ct);

//             Users? user;

//             if (identity != null)
//             {
//                 // User already linked
//                 user = await _userRepository.GetByIdAsync(identity.UserId, ct);
//                 if (user is null)
//                 {
//                     return Result.Failure<(Users, bool, bool)>(
//                         new Error("UserNotFound", "Linked user not found."));
//                 }
//             }
//             else
//             {
//                 // Try to find by email
//                 user = !string.IsNullOrWhiteSpace(payload.Email)
//                     ? await _userRepository.GetByEmailAsync(payload.Email, ct)
//                     : null;

//                 if (user is null)
//                 {
//                     // Create new user
//                     user = CreateUserFromPayload(payload);
//                     user.AddExternalIdentity(AuthProvider.Google, payload.Subject);
//                     await _userRepository.AddAsync(user, ct);
//                     isNewUser = true;
//                 }
//                 else
//                 {
//                     // Existing user, link external identity
//                     user.AddExternalIdentity(AuthProvider.Google, payload.Subject);
//                     needsUpdate = true;
//                 }
//             }

//             return Result.Success((user, isNewUser, needsUpdate));
//         }

//         private static Users CreateUserFromPayload(GoogleJsonWebSignature.Payload payload)
//         {
//             var userName = BuildUserNameFromEmail(payload.Email);

//             return Users.Create(
//                 email: payload.Email,
//                 userName: userName,
//                 passwordHash: string.Empty, // External accounts don't need local password
//                 firstName: payload.GivenName,
//                 lastName: payload.FamilyName,
//                 profileImageUrl: payload.Picture
//             );
//         }

//         private (string RefreshToken, DateTime RefreshExpiry, bool NeedsUpdate) GenerateOrReuseRefreshToken(
//             Users user)
//         {
//             var hasValidRefreshToken = !string.IsNullOrEmpty(user.RefreshToken)
//                 && user.RefreshTokenExpiry.HasValue
//                 && user.RefreshTokenExpiry.Value > DateTime.UtcNow;

//             if (hasValidRefreshToken)
//             {
//                 return (user.RefreshToken!, user.RefreshTokenExpiry!.Value, false);
//             }

//             var newToken = _jwtTokenService.GenerateRefreshToken();
//             var newExpiry = DateTime.UtcNow.AddDays(_jwtTokenService.RefreshTokenExpiryDays);

//             return (newToken, newExpiry, true);
//         }

//         private static string BuildUserNameFromEmail(string? email)
//         {
//             if (string.IsNullOrWhiteSpace(email))
//             {
//                 return $"user_{Guid.NewGuid():N}"[..GeneratedUsernameLength];
//             }

//             var local = email.Split('@', 2)[0];
//             return local.Length <= MaxUsernameLength
//                 ? local
//                 : local[..MaxUsernameLength];
//         }
//     }
// }