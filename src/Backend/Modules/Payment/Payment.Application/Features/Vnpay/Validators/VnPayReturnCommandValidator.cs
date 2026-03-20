// using FluentValidation;

// namespace Payments.Application.Validators;

// public class VnPayReturnCommandValidator : AbstractValidator<VnPayReturnCommand>
// {
//     private static readonly string[] RequiredKeys =
//     [
//         "vnp_TxnRef",
//         "vnp_ResponseCode",
//         "vnp_SecureHash",
//         "vnp_Amount",
//         "vnp_TransactionNo"
//     ];

//     public VnPayReturnCommandValidator()
//     {
//         RuleFor(x => x.QueryParams)
//             .NotNull()
//             .WithMessage("VNPay callback parameters are required.");

//         RuleFor(x => x.QueryParams)
//             .Must(params_ => RequiredKeys.All(k => params_.ContainsKey(k) && !string.IsNullOrEmpty(params_[k])))
//             .WithMessage("Missing required VNPay callback parameters.")
//             .When(x => x.QueryParams != null);

//         RuleFor(x => x.QueryParams)
//             .Must(params_ => params_.TryGetValue("vnp_Amount", out var a) && long.TryParse(a, out var v) && v > 0)
//             .WithMessage("VNPay amount must be a valid positive number.")
//             .When(x => x.QueryParams != null && x.QueryParams.ContainsKey("vnp_Amount"));
//     }
// }
