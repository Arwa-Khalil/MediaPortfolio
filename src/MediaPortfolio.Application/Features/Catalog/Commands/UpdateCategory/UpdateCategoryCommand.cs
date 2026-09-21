using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Catalog.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}
