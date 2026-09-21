using System;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetCategories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int VideoCount { get; set; }
}
