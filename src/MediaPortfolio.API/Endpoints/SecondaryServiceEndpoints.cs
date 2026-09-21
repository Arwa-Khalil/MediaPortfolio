using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Services.Commands.CreateSecondaryService;
using MediaPortfolio.Application.Features.Services.Commands.UpdateSecondaryService;
using MediaPortfolio.Application.Features.Services.Commands.DeleteSecondaryService;
using MediaPortfolio.Application.Features.Services.Queries.GetSecondaryServices;
using MediaPortfolio.API.Extensions;

namespace MediaPortfolio.API.Endpoints;

public static class SecondaryServiceEndpoints
{
    public static void MapSecondaryServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/v1/admin/secondary-services")
            .WithTags("Secondary Services (Admin)")
            .RequireAuthorization(); // Requires JWT

        var publicGroup = app.MapGroup("/api/v1/secondary-services")
            .WithTags("Secondary Services (Public)");

        // Public
        publicGroup.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetSecondaryServicesQuery());
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        // Admin
        adminGroup.MapPost("/", async (ISender sender, CreateSecondaryServiceCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Created($"/api/v1/secondary-services/{result.Value}", result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        adminGroup.MapPut("/{id:guid}", async (Guid id, ISender sender, UpdateSecondaryServiceCommand command) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest(MediaPortfolio.API.Models.ApiResponse<object>.Failure("FIELD_VALIDATION_FAILED", "ID in URL must match ID in body."));
            }

            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        adminGroup.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteSecondaryServiceCommand { Id = id };
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.ToApiResponse());
        });
    }
}
