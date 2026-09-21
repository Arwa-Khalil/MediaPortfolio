using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MediaPortfolio.API.Endpoints;

public static class LocalMediaEndpoints
{
    public static void MapLocalMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/local-media")
            .WithTags("Local Media");

        group.MapPost("/upload/video/{assetId}", async (string assetId, HttpRequest request) =>
        {
            var dir = Path.Combine("/media-storage", "videos");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, assetId);
            
            using var fs = new FileStream(filePath, FileMode.Create);
            await request.Body.CopyToAsync(fs);
            
            return Results.Ok(new { assetId });
        }).DisableAntiforgery();

        group.MapPost("/upload/image/{assetId}", async (string assetId, HttpRequest request) =>
        {
            var dir = Path.Combine("/media-storage", "images");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, assetId);
            
            using var fs = new FileStream(filePath, FileMode.Create);
            await request.Body.CopyToAsync(fs);
            
            return Results.Ok(new { assetId });
        }).DisableAntiforgery();

        group.MapGet("/video/{assetId}", (string assetId) =>
        {
            var filePath = Path.Combine("/media-storage", "videos", assetId);
            if (!File.Exists(filePath)) return Results.NotFound();
            return Results.File(filePath, "video/mp4");
        });

        group.MapGet("/image/{assetId}", (string assetId) =>
        {
            var filePath = Path.Combine("/media-storage", "images", assetId);
            if (!File.Exists(filePath)) return Results.NotFound();
            return Results.File(filePath, "image/jpeg");
        });
        
        group.MapGet("/placeholder-thumbnail.png", () =>
        {
            return Results.Content("Dummy Thumbnail", "text/plain");
        });
    }
}
