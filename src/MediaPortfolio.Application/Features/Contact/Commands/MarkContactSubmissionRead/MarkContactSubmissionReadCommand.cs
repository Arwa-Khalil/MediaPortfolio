using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Contact.Commands.MarkContactSubmissionRead;

public class MarkContactSubmissionReadCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
}
