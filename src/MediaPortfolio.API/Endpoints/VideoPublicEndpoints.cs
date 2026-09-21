using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Catalog.Queries.GetPublicVideos;
using MediaPortfolio.Application.Features.Catalog.Queries.SearchPublicVideos;
using MediaPortfolio.Application.Features.Catalog.Queries.GetPublicVideoById;
using MediaPortfolio.Application.Features.Catalog.Commands.TrackVideoView;
using MediaPortfolio.Application.Features.Catalog.Commands.TrackWhatsAppClick;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.API.Extensions;
using MediaPortfolio.API.Models;
using System.Collections.Generic;

namespace MediaPortfolio.API.Endpoints;

public static class VideoPublicEndpoints
{
    public static void MapVideoPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/videos")
            .WithTags("Videos (Public)");

        group.MapGet("/", async ([AsParameters] GetPublicVideosQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(result.ToApiResponse());
            }

            var apiResponse = ApiResponse<IEnumerable<PublicVideoDto>>.Success(
                result.Value!.Items, 
                new PaginationMeta(query.Page.GetValueOrDefault(1), query.PageSize.GetValueOrDefault(20), result.Value.TotalCount)
            );

            return Results.Ok(apiResponse);
        });

        // GET /api/v1/videos/search
        group.MapGet("/search", async ([AsParameters] SearchPublicVideosQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(result.ToApiResponse());
            }

            var apiResponse = ApiResponse<IEnumerable<PublicVideoDto>>.Success(
                result.Value!.Items,
                new PaginationMeta(query.Page.GetValueOrDefault(1), query.PageSize.GetValueOrDefault(20), result.Value.TotalCount)
            );

            return Results.Ok(apiResponse);
        });

        // GET /api/v1/videos/{id}
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetPublicVideoByIdQuery { Id = id };
            var result = await sender.Send(query);
            
            if (!result.IsSuccess)
            {
                // Returns identical 404 (RESOURCE_NOT_FOUND) for Draft, Deleted, or Missing
                return Results.NotFound(result.ToApiResponse());
            }

            return Results.Ok(result.ToApiResponse());
        });

        // POST /api/v1/videos/{id}/track-view
        group.MapPost("/{id:guid}/track-view", async (Guid id, ISender sender) =>
        {
            var command = new TrackVideoViewCommand { VideoId = id };
            var result = await sender.Send(command);
            
            if (!result.IsSuccess)
            {
                return Results.NotFound(result.ToApiResponse());
            }

            // "200, empty data"
            return Results.Ok(ApiResponse<object>.Success(null!));
        }).RequireRateLimiting("TrackingEndpoints");

        // POST /api/v1/videos/{id}/track-whatsapp-click
        group.MapPost("/{id:guid}/track-whatsapp-click", async (Guid id, ISender sender) =>
        {
            var command = new TrackWhatsAppClickCommand { VideoId = id };
            var result = await sender.Send(command);
            
            if (!result.IsSuccess)
            {
                return Results.NotFound(result.ToApiResponse());
            }

            return Results.Ok(ApiResponse<object>.Success(null!));
        }).RequireRateLimiting("TrackingEndpoints");
    }
}
