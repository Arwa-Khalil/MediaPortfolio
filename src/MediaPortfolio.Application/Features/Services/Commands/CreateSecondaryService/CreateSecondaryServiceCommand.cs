using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Services.Commands.CreateSecondaryService;

public class CreateSecondaryServiceCommand : IRequest<Result<Guid>>
{
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public int? DisplayOrder { get; set; }
}
