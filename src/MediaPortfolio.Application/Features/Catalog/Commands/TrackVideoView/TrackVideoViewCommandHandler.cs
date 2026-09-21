using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Commands.TrackVideoView;

public class TrackVideoViewCommandHandler : IRequestHandler<TrackVideoViewCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public TrackVideoViewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(
        TrackVideoViewCommand request,
        CancellationToken cancellationToken)
    {
        // Anti-leak: same RESOURCE_NOT_FOUND whether video is Draft, deleted, or nonexistent.
        // We check existence first (lightweight query), then do the atomic increment
        // via ExecuteUpdateAsync to avoid a read-modify-write race condition.
        var exists = await _context.Videos
            .AnyAsync(
                v => v.Id == request.VideoId
                     && v.Status == VideoStatus.Published
                     && !v.IsDeleted,
                cancellationToken);

        if (!exists)
        {
            return Result<Unit>.Failure("RESOURCE_NOT_FOUND", "Video not found.");
        }

        // Atomic server-side increment — no concurrency issue.
        await _context.Videos
            .Where(v => v.Id == request.VideoId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(v => v.ViewCount, v => v.ViewCount + 1),
                cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
