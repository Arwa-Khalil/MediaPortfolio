using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Contact.Commands.DeleteContactSubmission;

public class DeleteContactSubmissionCommandHandler : IRequestHandler<DeleteContactSubmissionCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public DeleteContactSubmissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(DeleteContactSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _context.ContactSubmissions
            .FirstOrDefaultAsync(cs => cs.Id == request.Id, cancellationToken);

        if (submission == null)
        {
            return Result<Unit>.Failure("RESOURCE_NOT_FOUND", "Contact submission not found.");
        }

        _context.ContactSubmissions.Remove(submission);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
