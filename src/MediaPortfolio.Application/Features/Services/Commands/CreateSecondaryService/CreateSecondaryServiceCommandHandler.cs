using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Services.Commands.CreateSecondaryService;

public class CreateSecondaryServiceCommandHandler : IRequestHandler<CreateSecondaryServiceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateSecondaryServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateSecondaryServiceCommand request, CancellationToken cancellationToken)
    {
        int displayOrder = request.DisplayOrder ?? 0;

        if (!request.DisplayOrder.HasValue)
        {
            var maxDisplayOrder = await _context.SecondaryServices
                .MaxAsync(s => (int?)s.DisplayOrder, cancellationToken);
            
            displayOrder = (maxDisplayOrder ?? -1) + 1;
        }

        var service = new SecondaryService
        {
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            DisplayOrder = displayOrder
        };

        _context.SecondaryServices.Add(service);

        return Result<Guid>.Success(service.Id);
    }
}
