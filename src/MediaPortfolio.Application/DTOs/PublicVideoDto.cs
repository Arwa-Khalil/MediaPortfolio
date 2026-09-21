using System;
using System.Collections.Generic;

namespace MediaPortfolio.Application.DTOs;

/// <summary>
/// Public wire shape for a video — intentionally omits TitleAr and DescriptionAr.
/// These admin-only fields must never appear as JSON keys in a public response,
/// even as null. A separate DTO (vs JsonIgnore on AdminVideoDto) is the only
/// compile-time guarantee that the serializer never emits them.
/// See API-Contract-v1.1.docx §4.3.
/// </summary>
public record PublicVideoDto
{
    public Guid Id { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string CloudflareStreamId { get; init; } = default!;
    public List<VideoCategoryDto> Categories { get; init; } = new();
}
