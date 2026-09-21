using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Media.Commands.GetVideoUploadUrl;
using MediaPortfolio.Application.Features.Media.Commands.GetImageUploadUrl;
using MediaPortfolio.API.Extensions;

namespace MediaPortfolio.API.Endpoints;

public static class MediaUploadEndpoints
{
    public static void MapMediaUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/media")
            .WithTags("Media Uploads (Admin)")
            .RequireAuthorization();

        group.MapPost("/video-upload-url", async (ISender sender) =>
        {
            var result = await sender.Send(new GetVideoUploadUrlCommand());
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        group.MapPost("/image-upload-url", async (ISender sender) =>
        {
            var result = await sender.Send(new GetImageUploadUrlCommand());
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });
    }
}
