using System;
using System.Collections.Generic;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Commands.CreateVideo;

public class CreateVideoCommand : IRequest<Result<Guid>>
{
    public string TitleAr { get; set; } = default!;
    public string DescriptionAr { get; set; } = default!;
    public List<Guid> CategoryIds { get; set; } = new();
    public string CloudflareStreamId { get; set; } = default!;
    public string? CloudflareThumbnailImageId { get; set; }
    public VideoStatus Status { get; set; }
}
