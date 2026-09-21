using System;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Contact.Queries.GetAdminContactSubmissions;

public class GetAdminContactSubmissionsQuery : IRequest<Result<PagedResult<ContactSubmissionDto>>>
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public ContactSubmissionStatus? Status { get; set; }
}
