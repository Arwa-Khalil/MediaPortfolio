using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Catalog.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result<Unit>.Failure("RESOURCE_NOT_FOUND", "The requested Category ID does not exist.");
        }

        // Check for duplicates excluding the current category
        var exists = await _context.Categories
            .AnyAsync(c => c.Id != request.Id && (c.NameEn == request.NameEn || c.NameAr == request.NameAr), cancellationToken);

        if (exists)
        {
            return Result<Unit>.Failure("DUPLICATE_CATEGORY_NAME", "A category with this name already exists.");
        }

        category.NameEn = request.NameEn;
        category.NameAr = request.NameAr;

        return Result<Unit>.Success(Unit.Value);
    }
}
