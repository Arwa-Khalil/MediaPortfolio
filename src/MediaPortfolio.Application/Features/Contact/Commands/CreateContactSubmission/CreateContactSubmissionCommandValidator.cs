using FluentValidation;

namespace MediaPortfolio.Application.Features.Contact.Commands.CreateContactSubmission;

public class CreateContactSubmissionCommandValidator : AbstractValidator<CreateContactSubmissionCommand>
{
    public CreateContactSubmissionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not in a valid format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.");

        RuleFor(x => x.TurnstileToken)
            .NotEmpty().WithMessage("TurnstileToken is required.");
    }
}
