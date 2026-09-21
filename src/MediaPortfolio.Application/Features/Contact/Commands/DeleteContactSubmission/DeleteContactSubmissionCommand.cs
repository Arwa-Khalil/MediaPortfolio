using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Contact.Commands.DeleteContactSubmission;

public class DeleteContactSubmissionCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
}
