using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Services.Commands.DeleteSecondaryService;

public class DeleteSecondaryServiceCommandHandler : IRequestHandler<DeleteSecondaryServiceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteSecondaryServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteSecondaryServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.SecondaryServices.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service == null)
        {
            return Result<Unit>.Failure("RESOURCE_NOT_FOUND", "The requested Secondary Service ID does not exist.");
        }

        // Hard delete the secondary service
        _context.SecondaryServices.Remove(service);

        return Result<Unit>.Success(Unit.Value);
    }
}
