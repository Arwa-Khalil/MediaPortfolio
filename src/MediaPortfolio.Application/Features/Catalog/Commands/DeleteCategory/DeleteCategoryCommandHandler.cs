using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Catalog.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result<Unit>.Failure("RESOURCE_NOT_FOUND", "The requested Category ID does not exist.");
        }

        // Soft delete the category
        category.IsDeleted = true;

        // Remove the join rows (VideoCategories) for this category.
        // This does NOT delete the Videos themselves, satisfying the non-cascade rule.
        var joinsToRemove = await _context.VideoCategories
            .Where(vc => vc.CategoryId == request.Id)
            .ToListAsync(cancellationToken);

        _context.VideoCategories.RemoveRange(joinsToRemove);

        return Result<Unit>.Success(Unit.Value);
    }
}
