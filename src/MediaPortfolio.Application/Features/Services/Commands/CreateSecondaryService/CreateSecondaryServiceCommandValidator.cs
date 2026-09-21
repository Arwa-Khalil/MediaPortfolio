using FluentValidation;

namespace MediaPortfolio.Application.Features.Services.Commands.CreateSecondaryService;

public class CreateSecondaryServiceCommandValidator : AbstractValidator<CreateSecondaryServiceCommand>
{
    public CreateSecondaryServiceCommandValidator()
    {
        RuleFor(x => x.TitleEn)
            .NotEmpty().WithMessage("English title is required.")
            .MaximumLength(255).WithMessage("English title must not exceed 255 characters.");

        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("Arabic title is required.")
            .MaximumLength(255).WithMessage("Arabic title must not exceed 255 characters.");

        RuleFor(x => x.DescriptionEn)
            .NotEmpty().WithMessage("English description is required.")
            .MaximumLength(1000).WithMessage("English description must not exceed 1000 characters.");

        RuleFor(x => x.DescriptionAr)
            .NotEmpty().WithMessage("Arabic description is required.")
            .MaximumLength(1000).WithMessage("Arabic description must not exceed 1000 characters.");
    }
}
