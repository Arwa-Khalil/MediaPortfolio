using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using Xunit;
using MediaPortfolio.Application.Features.Catalog.Commands.CreateVideo;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.UnitTests.Features.Catalog.Commands;

public class CreateVideoCommandValidatorTests
{
    private readonly CreateVideoCommandValidator _validator;

    public CreateVideoCommandValidatorTests()
    {
        _validator = new CreateVideoCommandValidator();
    }

    [Fact]
    public void Should_HaveError_When_TitleAr_IsEmpty()
    {
        var model = new CreateVideoCommand { TitleAr = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TitleAr);
    }

    [Fact]
    public void Should_HaveError_When_CategoryIds_IsEmpty()
    {
        var model = new CreateVideoCommand { CategoryIds = new List<Guid>() };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CategoryIds);
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsComplete()
    {
        var model = new CreateVideoCommand
        {
            TitleAr = "Test Title",
            DescriptionAr = "Test Desc",
            CategoryIds = new List<Guid> { Guid.NewGuid() },
            CloudflareStreamId = "test-stream-id",
            Status = VideoStatus.Draft
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
