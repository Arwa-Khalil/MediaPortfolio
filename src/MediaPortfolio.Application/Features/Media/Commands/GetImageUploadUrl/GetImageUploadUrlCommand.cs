using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Media.Commands.GetImageUploadUrl;

public class GetImageUploadUrlCommand : IRequest<Result<MediaImageUploadUrlResultDto>>
{
}

public class GetImageUploadUrlCommandHandler : IRequestHandler<GetImageUploadUrlCommand, Result<MediaImageUploadUrlResultDto>>
{
    private readonly IImageStorageService _imageStorageService;

    public GetImageUploadUrlCommandHandler(IImageStorageService imageStorageService)
    {
        _imageStorageService = imageStorageService;
    }

    public async Task<Result<MediaImageUploadUrlResultDto>> Handle(GetImageUploadUrlCommand request, CancellationToken cancellationToken)
    {
        var result = await _imageStorageService.GetUploadUrlAsync();
        return Result<MediaImageUploadUrlResultDto>.Success(new MediaImageUploadUrlResultDto(result.UploadUrl, result.AssetId));
    }
}
