using System;

namespace MediaPortfolio.Application.Features.Services.Queries.GetSecondaryServices;

public class SecondaryServiceDto
{
    public Guid Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
