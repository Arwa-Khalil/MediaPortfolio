using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Catalog.Commands.CreateVideo;
using MediaPortfolio.Application.Features.Catalog.Commands.UpdateVideo;
using MediaPortfolio.Application.Features.Catalog.Commands.UpdateVideoStatus;
using MediaPortfolio.Application.Features.Catalog.Commands.DeleteVideo;
using MediaPortfolio.Application.Features.Catalog.Queries.GetAdminVideos;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.API.Extensions;
using MediaPortfolio.API.Models;
using System.Collections.Generic;

namespace MediaPortfolio.API.Endpoints;

public static class VideoAdminEndpoints
{
    public static void MapVideoAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/videos")
            .WithTags("Videos (Admin)")
            .RequireAuthorization();

        group.MapGet("/", async ([AsParameters] GetAdminVideosQuery query, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(query);
                if (!result.IsSuccess)
                {
                    return Results.BadRequest(result.ToApiResponse());
                }

                var apiResponse = ApiResponse<IEnumerable<AdminVideoDto>>.Success(result.Value!.Items, new PaginationMeta(query.Page.GetValueOrDefault(1), query.PageSize.GetValueOrDefault(20), result.Value.TotalCount));

                return Results.Ok(apiResponse);
            }
            catch (Exception ex)
            {
                return Results.InternalServerError(ex.ToString());
            }
        });

        group.MapPost("/", async (ISender sender, CreateVideoCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Created($"/api/v1/admin/videos/{result.Value}", result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        group.MapPut("/{id:guid}", async (Guid id, ISender sender, UpdateVideoCommand command) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest(ApiResponse<object>.Failure("FIELD_VALIDATION_FAILED", "ID in URL must match ID in body.", "id"));
            }

            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        group.MapPatch("/{id:guid}/status", async (Guid id, ISender sender, UpdateVideoStatusCommand command) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest(ApiResponse<object>.Failure("FIELD_VALIDATION_FAILED", "ID in URL must match ID in body.", "id"));
            }

            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteVideoCommand { Id = id };
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.ToApiResponse());
        });
    }
}
