using System;

namespace MediaPortfolio.Domain.Entities;

public class VideoCategory
{
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
