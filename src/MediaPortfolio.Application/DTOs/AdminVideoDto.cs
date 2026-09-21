using System;
using System.Collections.Generic;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.DTOs;

public record AdminVideoDto
{
    public Guid Id { get; init; }
    public string TitleAr { get; init; } = default!;
    public string DescriptionAr { get; init; } = default!;
    public string CloudflareStreamId { get; init; } = default!;
    public string? CloudflareThumbnailImageId { get; init; }
    public string? ThumbnailUrl { get; init; }
    public VideoStatus Status { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public int ViewCount { get; init; }
    public int WhatsAppClickCount { get; init; }
    public int FormSubmissionCount { get; init; }
    public List<VideoCategoryDto> Categories { get; init; } = new();
}
