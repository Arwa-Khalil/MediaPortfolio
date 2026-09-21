using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Contact.Commands.CreateContactSubmission;

public class CreateContactSubmissionCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Message { get; set; } = default!;
    public Guid? OriginCategoryId { get; set; }
    public Guid? OriginVideoId { get; set; }
    public string TurnstileToken { get; set; } = default!;
    public string? RemoteIp { get; set; }
}
