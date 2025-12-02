// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Options;
// using Shared.Infrastructure.Configs;

// namespace Shared.Infrastructure.Configs.Payments
// {
//     public class VnPayConfigSetup : IConfigureOptions<VnPayConfig>
//     {
//         private readonly IConfiguration _configuration;

//         public VnPayConfigSetup(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         public void Configure(VnPayConfig options)
//         {
//             // Get the VNPay section
//             var vnpaySection = _configuration.GetSection("VNPay");
            
//             if (!vnpaySection.Exists())
//             {
//                 Console.WriteLine("WARNING: VNPay section not found in configuration!");
//                 return;
//             }

//             // Bind manually to ensure values are set
//             options.Url = vnpaySection["Url"] ?? string.Empty;
//             options.TmnCode = vnpaySection["TmnCode"] ?? string.Empty;
//             options.HashSecret = vnpaySection["HashSecret"] ?? string.Empty;
//             options.ReturnUrl = vnpaySection["ReturnUrl"] ?? string.Empty;
            
//             // Log to verify
//             Console.WriteLine("=== VNPay Configuration Loaded ===");
//             Console.WriteLine($"Url: {options.Url}");
//             Console.WriteLine($"TmnCode: {options.TmnCode}");
//             Console.WriteLine($"ReturnUrl: {options.ReturnUrl}");
//             Console.WriteLine($"HashSecret: {(!string.IsNullOrEmpty(options.HashSecret) ? "***SET***" : "***NOT SET***")}");
//             Console.WriteLine("==================================");
//         }
//     }
// }