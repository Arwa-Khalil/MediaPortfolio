using System;

namespace MediaPortfolio.Application.DTOs;

public record VideoCategoryDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = default!;
    public string NameAr { get; init; } = default!;
}
