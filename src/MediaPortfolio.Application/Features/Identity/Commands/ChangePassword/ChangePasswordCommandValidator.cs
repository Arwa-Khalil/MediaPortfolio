using FluentValidation;

namespace MediaPortfolio.Application.Features.Identity.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.");
            
        RuleFor(v => v.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");
            
        RuleFor(v => v.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .NotEqual(v => v.CurrentPassword).WithMessage("New password cannot be the same as the current password.");
    }
}
