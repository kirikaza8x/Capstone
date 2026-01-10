// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Options;

// namespace Shared.Infrastructure.Configs.Security
// {
//     public class GoogleAuthConfigSetup : IConfigureOptions<GoogleAuthConfigs>
//     {
//         private readonly IConfiguration _configuration;

//         public GoogleAuthConfigSetup(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         public void Configure(GoogleAuthConfigs options)
//         {
//             _configuration.GetSection("Authentication:Google").Bind(options);
//         }
//     }

//     // Optional: Add validation
//     public class GoogleAuthConfigValidation : IValidateOptions<GoogleAuthConfigs>
//     {
//         public ValidateOptionsResult Validate(string? name, GoogleAuthConfigs options)
//         {
//             if (string.IsNullOrWhiteSpace(options.ServerClientId))
//             {
//                 return ValidateOptionsResult.Fail(
//                     "Google ServerClientId is required. Configure 'Authentication:Google:ServerClientId' in appsettings.json");
//             }

//             if (!options.ServerClientId.EndsWith(".apps.googleusercontent.com"))
//             {
//                 return ValidateOptionsResult.Fail(
//                     "Invalid Google ServerClientId format. It should end with '.apps.googleusercontent.com'");
//             }

//             return ValidateOptionsResult.Success;
//         }
//     }
// }