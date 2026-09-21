using FluentValidation;

namespace MediaPortfolio.Application.Features.Catalog.Commands.UpdateVideoStatus;

public class UpdateVideoStatusCommandValidator : AbstractValidator<UpdateVideoStatusCommand>
{
    public UpdateVideoStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");
    }
}
