using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Commands.UpdateVideoStatus;

public class UpdateVideoStatusCommandHandler : IRequestHandler<UpdateVideoStatusCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public UpdateVideoStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(UpdateVideoStatusCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (video == null)
        {
            return Result<Guid>.Failure("RESOURCE_NOT_FOUND", "Video not found.");
        }

        if (video.Status != request.Status)
        {
            video.Status = request.Status;
            // Requirement 3: Set PublishedAt fresh every time it transitions to Published, clear when Draft.
            video.PublishedAt = request.Status == VideoStatus.Published ? DateTimeOffset.UtcNow : null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(video.Id);
    }
}
