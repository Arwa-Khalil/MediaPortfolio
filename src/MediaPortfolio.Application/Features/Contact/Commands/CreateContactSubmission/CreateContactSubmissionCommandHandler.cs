using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Domain.Enums;
using System.Linq;

namespace MediaPortfolio.Application.Features.Contact.Commands.CreateContactSubmission;

public class CreateContactSubmissionCommandHandler : IRequestHandler<CreateContactSubmissionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITurnstileVerificationService _turnstileService;

    public CreateContactSubmissionCommandHandler(
        IApplicationDbContext context,
        ITurnstileVerificationService turnstileService)
    {
        _context = context;
        _turnstileService = turnstileService;
    }

    public async Task<Result<Guid>> Handle(CreateContactSubmissionCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Turnstile token
        var isTurnstileValid = await _turnstileService.VerifyAsync(request.TurnstileToken, request.RemoteIp, cancellationToken);
        if (!isTurnstileValid)
        {
            var errors = new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "turnstileToken", new[] { "Turnstile verification failed." } }
            };
            throw new MediaPortfolio.Application.Exceptions.ValidationException(errors);
        }

        // 2. Validate OriginCategoryId
        Guid? validCategoryId = null;
        if (request.OriginCategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.OriginCategoryId.Value && !c.IsDeleted, cancellationToken);
            if (categoryExists)
            {
                validCategoryId = request.OriginCategoryId.Value;
            }
        }

        // 3. Validate OriginVideoId
        Guid? validVideoId = null;
        if (request.OriginVideoId.HasValue)
        {
            var videoExists = await _context.Videos
                .AnyAsync(v => v.Id == request.OriginVideoId.Value && v.Status == VideoStatus.Published && !v.IsDeleted, cancellationToken);
            if (videoExists)
            {
                validVideoId = request.OriginVideoId.Value;
            }
        }

        // 4. Create the entity
        var submission = new ContactSubmission
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Message = request.Message,
            OriginCategoryId = validCategoryId,
            OriginVideoId = validVideoId,
            Status = ContactSubmissionStatus.Unread,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.ContactSubmissions.Add(submission);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Increment video counter if valid
        if (validVideoId.HasValue)
        {
            await _context.Videos
                .Where(v => v.Id == validVideoId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.FormSubmissionCount, v => v.FormSubmissionCount + 1), cancellationToken);
        }

        return Result<Guid>.Success(submission.Id);
    }
}
