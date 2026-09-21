using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;
using MediaPortfolio.Application.Behaviors;
using ValidationException = MediaPortfolio.Application.Exceptions.ValidationException;

namespace MediaPortfolio.Application.UnitTests;

public class ValidationBehaviorTests
{
    public class TestRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_WithValidationErrors_ThrowsValidationException()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        var failures = new List<ValidationFailure> { new ValidationFailure("Property", "Error message") };
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validatorMock.Object });
        
        var nextMock = new Mock<RequestHandlerDelegate<string>>();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(new TestRequest(), nextMock.Object, CancellationToken.None));
        Assert.Contains(ex.Errors, e => e.Key == "Property" && e.Value[0] == "Error message");
        nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoValidationErrors_CallsNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validatorMock.Object });
        
        var nextMock = new Mock<RequestHandlerDelegate<string>>();
        nextMock.Setup(n => n()).ReturnsAsync("Success");

        // Act
        var result = await behavior.Handle(new TestRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        Assert.Equal("Success", result);
        nextMock.Verify(n => n(), Times.Once);
    }
}
