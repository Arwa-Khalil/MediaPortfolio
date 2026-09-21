using FluentValidation.TestHelper;
using MediaPortfolio.Application.Features.Contact.Commands.CreateContactSubmission;
using Xunit;

namespace MediaPortfolio.Application.UnitTests.Features.Contact.Commands;

public class CreateContactSubmissionCommandValidatorTests
{
    private readonly CreateContactSubmissionCommandValidator _validator;

    public CreateContactSubmissionCommandValidatorTests()
    {
        _validator = new CreateContactSubmissionCommandValidator();
    }

    [Fact]
    public void Should_HaveError_When_Name_IsEmpty()
    {
        var model = new CreateContactSubmissionCommand { Name = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_Email_IsInvalid()
    {
        var model = new CreateContactSubmissionCommand { Email = "not-an-email" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_When_TurnstileToken_IsEmpty()
    {
        var model = new CreateContactSubmissionCommand { TurnstileToken = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TurnstileToken);
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsComplete()
    {
        var model = new CreateContactSubmissionCommand
        {
            Name = "John Doe",
            Phone = "+1234567890",
            Email = "john@example.com",
            Message = "Hello World",
            TurnstileToken = "valid-token"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
