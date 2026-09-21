using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Media.Commands.GetVideoUploadUrl;

public class GetVideoUploadUrlCommand : IRequest<Result<MediaUploadUrlResultDto>>
{
}

public class GetVideoUploadUrlCommandHandler : IRequestHandler<GetVideoUploadUrlCommand, Result<MediaUploadUrlResultDto>>
{
    private readonly IVideoStorageService _videoStorageService;

    public GetVideoUploadUrlCommandHandler(IVideoStorageService videoStorageService)
    {
        _videoStorageService = videoStorageService;
    }

    public async Task<Result<MediaUploadUrlResultDto>> Handle(GetVideoUploadUrlCommand request, CancellationToken cancellationToken)
    {
        var result = await _videoStorageService.GetUploadUrlAsync();
        return Result<MediaUploadUrlResultDto>.Success(new MediaUploadUrlResultDto(result.UploadUrl, result.AssetId));
    }
}
