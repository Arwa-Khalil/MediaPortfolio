using System;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.DTOs;

public record ContactSubmissionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Message { get; init; } = default!;
    
    public VideoCategoryDto? OriginCategory { get; init; }
    public OriginVideoSummaryDto? OriginVideo { get; init; }
    
    public ContactSubmissionStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}
