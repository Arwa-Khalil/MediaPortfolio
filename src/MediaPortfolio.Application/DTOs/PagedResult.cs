using System.Collections.Generic;

namespace MediaPortfolio.Application.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
}
