using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Services.Commands.UpdateSecondaryService;

public class UpdateSecondaryServiceCommandHandler : IRequestHandler<UpdateSecondaryServiceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateSecondaryServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateSecondaryServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.SecondaryServices.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            return Result<Unit>.Failure("RESOURCE_NOT_FOUND", "The requested Secondary Service ID does not exist.");
        }

        service.TitleEn = request.TitleEn;
        service.TitleAr = request.TitleAr;
        service.DescriptionEn = request.DescriptionEn;
        service.DescriptionAr = request.DescriptionAr;
        service.DisplayOrder = request.DisplayOrder;

        return Result<Unit>.Success(Unit.Value);
    }
}
