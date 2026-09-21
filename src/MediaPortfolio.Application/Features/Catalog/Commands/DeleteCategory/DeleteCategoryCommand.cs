using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Catalog.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
}
