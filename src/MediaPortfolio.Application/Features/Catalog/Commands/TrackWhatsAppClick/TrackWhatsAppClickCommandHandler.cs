using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Commands.TrackWhatsAppClick;

public class TrackWhatsAppClickCommandHandler : IRequestHandler<TrackWhatsAppClickCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public TrackWhatsAppClickCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(
        TrackWhatsAppClickCommand request,
        CancellationToken cancellationToken)
    {
        // Anti-leak: same RESOURCE_NOT_FOUND whether video is Draft, deleted, or nonexistent.
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

        // Atomic server-side increment.
        await _context.Videos
            .Where(v => v.Id == request.VideoId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(v => v.WhatsAppClickCount, v => v.WhatsAppClickCount + 1),
                cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
