using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Catalog.Commands.DeleteVideo;

public class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteVideoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (video == null)
        {
            return Result<bool>.Failure("RESOURCE_NOT_FOUND", "Video not found.");
        }

        video.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
