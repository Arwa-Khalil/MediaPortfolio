using System;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Domain.Entities;

public class ContactSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public Guid? OriginCategoryId { get; set; }
    public Guid? OriginVideoId { get; set; }
    
    public ContactSubmissionStatus Status { get; set; } = ContactSubmissionStatus.Unread;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReadAt { get; set; }
}
