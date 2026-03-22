using FluentValidation;
using Payments.Application.Features.Payments.Commands.VnPayReturn;

namespace Payments.Application.Validators;

public class VnPayReturnCommandValidator : AbstractValidator<VnPayReturnCommand>
{
    private static readonly string[] RequiredKeys =
    [
        "vnp_TxnRef",
        "vnp_ResponseCode",
        "vnp_SecureHash",
        "vnp_Amount",
        "vnp_TransactionNo"
    ];

    public VnPayReturnCommandValidator()
    {
        RuleFor(x => x.QueryParams)
            .NotNull().WithMessage("Callback parameters are required.");

        RuleFor(x => x.QueryParams)
            .Must(p => RequiredKeys.All(k =>
                p.ContainsKey(k) && !string.IsNullOrWhiteSpace(p[k])))
            .WithMessage("Missing required VNPay callback parameters.")
            .When(x => x.QueryParams != null);

        RuleFor(x => x.QueryParams)
            .Must(p => p.TryGetValue("vnp_Amount", out var a)
                    && long.TryParse(a, out var v) && v > 0)
            .WithMessage("VNPay amount must be a valid positive number.")
            .When(x => x.QueryParams != null
                    && x.QueryParams.ContainsKey("vnp_Amount"));
    }
}