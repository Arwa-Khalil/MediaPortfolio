using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Contact.Commands.MarkContactSubmissionRead;

public class MarkContactSubmissionReadCommandHandler : IRequestHandler<MarkContactSubmissionReadCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public MarkContactSubmissionReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(MarkContactSubmissionReadCommand request, CancellationToken cancellationToken)
    {
        var submission = await _context.ContactSubmissions
            .FirstOrDefaultAsync(cs => cs.Id == request.Id, cancellationToken);

        if (submission == null)
        {
            return Result<Unit>.Failure("RESOURCE_NOT_FOUND", "Contact submission not found.");
        }

        if (submission.Status != ContactSubmissionStatus.Read)
        {
            submission.Status = ContactSubmissionStatus.Read;
            submission.ReadAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
