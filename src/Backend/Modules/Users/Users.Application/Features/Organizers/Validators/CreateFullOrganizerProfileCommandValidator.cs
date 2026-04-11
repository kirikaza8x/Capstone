using FluentValidation;

namespace Users.Application.Features.Organizers.Validators;

public class StartOrUpdateOrganizerProfileCommandValidator
    : AbstractValidator<StartOrUpdateOrganizerProfileCommand>
{
    private static readonly string[] AllowedContentTypes =
       { "image/jpeg", "image/png", "image/gif", "image/webp" };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    public StartOrUpdateOrganizerProfileCommandValidator()
    {
        // --------------------
        // Type
        // --------------------
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Organizer type must be a valid value.");

        // --------------------
        // Business Info (REQUIRED)
        // --------------------
        RuleFor(x => x.BusinessInfo)
            .NotNull()
            .WithMessage("Business information is required.");

        When(x => x.BusinessInfo != null, () =>
        {
            RuleFor(x => x.BusinessInfo.DisplayName)
                .NotEmpty()
                .WithMessage("Display name is required.")
                .MaximumLength(200);

            RuleFor(x => x.BusinessInfo.Description)
                .MaximumLength(1000);

            RuleFor(x => x.BusinessInfo.Address)
                .MaximumLength(300);

            RuleFor(x => x.BusinessInfo.SocialLink)
                .MaximumLength(300);

            RuleFor(x => x.BusinessInfo.BusinessType)
                .IsInEnum()
                .When(x => x.BusinessInfo.BusinessType.HasValue)
                .WithMessage("Business type must be valid.");

            RuleFor(x => x.BusinessInfo.TaxCode)
                .MaximumLength(50);

            RuleFor(x => x.BusinessInfo.IdentityNumber)
                .MaximumLength(50);

            RuleFor(x => x.BusinessInfo.CompanyName)
                .MaximumLength(200);
        });

        // --------------------
        // Bank Info (REQUIRED)
        // --------------------
        RuleFor(x => x.BankInfo)
            .NotNull()
            .WithMessage("Bank information is required.");

        When(x => x.BankInfo != null, () =>
        {
            RuleFor(x => x.BankInfo.AccountName)
                .NotEmpty()
                .WithMessage("Account name is required.")
                .MaximumLength(200);

            RuleFor(x => x.BankInfo.AccountNumber)
                .NotEmpty()
                .WithMessage("Account number is required.")
                .MaximumLength(50);

            RuleFor(x => x.BankInfo.BankCode)
                .NotEmpty()
                .WithMessage("Bank code is required.")
                .MaximumLength(50);

            RuleFor(x => x.BankInfo.Branch)
                .MaximumLength(200);
        });

        When(x => x.LogoFile != null, () =>
        {
            RuleFor(x => x.LogoFile)
                .Must(f => f!.Length > 0).WithMessage("Logo file cannot be empty.")
                .Must(f => f!.Length <= MaxFileSize).WithMessage("Logo file too large. Max size is 10 MB.")
                .Must(f => AllowedContentTypes.Contains(f!.ContentType.ToLowerInvariant()))
                .WithMessage("Invalid logo type. Allowed: jpeg, png, gif, webp.");
        });
    }
}