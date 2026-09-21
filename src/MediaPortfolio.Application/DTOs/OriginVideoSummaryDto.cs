using System;

namespace MediaPortfolio.Application.DTOs;

public record OriginVideoSummaryDto
{
    public Guid Id { get; init; }
    public string? ThumbnailUrl { get; init; }
}
