using System;
using System.Collections.Generic;

namespace MediaPortfolio.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }

    public ICollection<VideoCategory> VideoCategories { get; set; } = new List<VideoCategory>();
}
