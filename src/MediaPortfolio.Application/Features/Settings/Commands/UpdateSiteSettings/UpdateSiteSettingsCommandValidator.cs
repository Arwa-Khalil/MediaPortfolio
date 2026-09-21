using FluentValidation;

namespace MediaPortfolio.Application.Features.Settings.Commands.UpdateSiteSettings;

public class UpdateSiteSettingsCommandValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    public UpdateSiteSettingsCommandValidator()
    {
        // No fields are strictly required on PUT — a null value means "leave unchanged".
        // We validate format only when a value IS provided.

        When(x => x.SnapchatUrl != null, () =>
        {
            RuleFor(x => x.SnapchatUrl!)
                .MaximumLength(500)
                .WithMessage("SnapchatUrl must not exceed 500 characters.")
                .Must(BeAValidUrl)
                .WithMessage("SnapchatUrl must be a valid URL.");
        });

        When(x => x.TiktokUrl != null, () =>
        {
            RuleFor(x => x.TiktokUrl!)
                .MaximumLength(500)
                .WithMessage("TiktokUrl must not exceed 500 characters.")
                .Must(BeAValidUrl)
                .WithMessage("TiktokUrl must be a valid URL.");
        });

        When(x => x.InstagramUrl != null, () =>
        {
            RuleFor(x => x.InstagramUrl!)
                .MaximumLength(500)
                .WithMessage("InstagramUrl must not exceed 500 characters.")
                .Must(BeAValidUrl)
                .WithMessage("InstagramUrl must be a valid URL.");
        });

        When(x => x.WhatsAppNumber != null, () =>
        {
            RuleFor(x => x.WhatsAppNumber!)
                .Matches(@"^\+\d{7,15}$")
                .WithMessage("WhatsAppNumber must be in E.164 format (e.g. +966501234567).")
                .MaximumLength(20)
                .WithMessage("WhatsAppNumber must not exceed 20 characters.");
        });

        When(x => x.WhatsAppTemplateAr != null, () =>
        {
            RuleFor(x => x.WhatsAppTemplateAr!)
                .MaximumLength(1000)
                .WithMessage("WhatsAppTemplateAr must not exceed 1000 characters.");
        });

        When(x => x.WhatsAppTemplateEn != null, () =>
        {
            RuleFor(x => x.WhatsAppTemplateEn!)
                .MaximumLength(1000)
                .WithMessage("WhatsAppTemplateEn must not exceed 1000 characters.");
        });
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
