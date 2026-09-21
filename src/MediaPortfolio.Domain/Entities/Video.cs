using System;
using System.Collections.Generic;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Domain.Entities;

public class Video
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Admin-only search fields
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    
    // Cloudflare asset IDs
    public string CloudflareStreamId { get; set; } = string.Empty;
    public string? CloudflareThumbnailImageId { get; set; }
    
    public VideoStatus Status { get; set; } = VideoStatus.Draft;
    
    // Analytics
    public int ViewCount { get; set; }
    public int WhatsAppClickCount { get; set; }
    public int FormSubmissionCount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }
    
    public bool IsDeleted { get; set; }

    public ICollection<VideoCategory> VideoCategories { get; set; } = new List<VideoCategory>();
}
