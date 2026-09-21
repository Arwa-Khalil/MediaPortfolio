using FluentValidation;

namespace MediaPortfolio.Application.Features.Catalog.Commands.CreateVideo;

public class CreateVideoCommandValidator : AbstractValidator<CreateVideoCommand>
{
    public CreateVideoCommandValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("TitleAr is required.")
            .MaximumLength(255).WithMessage("TitleAr must not exceed 255 characters.");

        RuleFor(x => x.DescriptionAr)
            .NotEmpty().WithMessage("DescriptionAr is required.")
            .MaximumLength(1000).WithMessage("DescriptionAr must not exceed 1000 characters.");

        RuleFor(x => x.CloudflareStreamId)
            .NotEmpty().WithMessage("CloudflareStreamId is required.")
            .MaximumLength(100).WithMessage("CloudflareStreamId must not exceed 100 characters.");

        RuleFor(x => x.CloudflareThumbnailImageId)
            .MaximumLength(100).WithMessage("CloudflareThumbnailImageId must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.CloudflareThumbnailImageId));

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("At least one category is required.");
            
        RuleForEach(x => x.CategoryIds)
            .NotEmpty().WithMessage("Category ID must not be empty.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");
    }
}
