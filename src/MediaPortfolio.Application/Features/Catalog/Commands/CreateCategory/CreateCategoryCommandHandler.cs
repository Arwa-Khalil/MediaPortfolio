using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Catalog.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicates (soft-deleted ones are filtered out by default due to EF Core Query Filter)
        var exists = await _context.Categories
            .AnyAsync(c => c.NameEn == request.NameEn || c.NameAr == request.NameAr, cancellationToken);

        if (exists)
        {
            return Result<Guid>.Failure("DUPLICATE_CATEGORY_NAME", "A category with this name already exists.");
        }

        var category = new Category
        {
            NameEn = request.NameEn,
            NameAr = request.NameAr
        };

        _context.Categories.Add(category);

        return Result<Guid>.Success(category.Id);
    }
}
