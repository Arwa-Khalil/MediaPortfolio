using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Analytics.Queries.GetVideoAnalytics;
using MediaPortfolio.API.Extensions;
using MediaPortfolio.API.Models;
using MediaPortfolio.Application.DTOs;
using System.Collections.Generic;

namespace MediaPortfolio.API.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/v1/admin/analytics")
            .WithTags("Analytics (Admin)")
            .RequireAuthorization();

        adminGroup.MapGet("/videos", async (ISender sender) =>
        {
            var result = await sender.Send(new GetVideoAnalyticsQuery());
            if (!result.IsSuccess)
            {
                return Results.BadRequest(result.ToApiResponse());
            }

            // Endpoint is unpaginated
            var apiResponse = ApiResponse<IEnumerable<AdminVideoDto>>.Success(result.Value!);
            return Results.Ok(apiResponse);
        });
    }
}
