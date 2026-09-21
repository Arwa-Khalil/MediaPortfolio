using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Catalog.Commands.CreateCategory;
using MediaPortfolio.Application.Features.Catalog.Commands.UpdateCategory;
using MediaPortfolio.Application.Features.Catalog.Commands.DeleteCategory;
using MediaPortfolio.Application.Features.Catalog.Queries.GetCategories;
using MediaPortfolio.API.Extensions;

namespace MediaPortfolio.API.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/v1/admin/categories")
            .WithTags("Categories (Admin)")
            .RequireAuthorization(); // Requires JWT

        var publicGroup = app.MapGroup("/api/v1/categories")
            .WithTags("Categories (Public)");

        // Public
        publicGroup.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCategoriesQuery());
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        // Admin
        adminGroup.MapPost("/", async (ISender sender, CreateCategoryCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Created($"/api/v1/categories/{result.Value}", result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        adminGroup.MapPut("/{id:guid}", async (Guid id, ISender sender, UpdateCategoryCommand command) =>
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
            var command = new DeleteCategoryCommand { Id = id };
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.ToApiResponse());
        });
    }
}
