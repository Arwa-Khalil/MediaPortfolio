using System.Collections.Generic;
using Xunit;
using MediaPortfolio.Domain.Exceptions;

namespace MediaPortfolio.Domain.UnitTests;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_SetsDefaultCode()
    {
        // Arrange
        var message = "User not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal("RESOURCE_NOT_FOUND", exception.Code);
        Assert.Equal(message, exception.Message);
    }
}
