using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Contact.Commands.CreateContactSubmission;
using MediaPortfolio.Application.Features.Contact.Queries.GetAdminContactSubmissions;
using MediaPortfolio.Application.Features.Contact.Commands.MarkContactSubmissionRead;
using MediaPortfolio.Application.Features.Contact.Commands.DeleteContactSubmission;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.API.Extensions;
using MediaPortfolio.API.Models;
using System.Collections.Generic;

namespace MediaPortfolio.API.Endpoints;

public static class ContactEndpoints
{
    public static void MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/v1/contact-submissions")
            .WithTags("Contact Submissions (Public)")
            .RequireRateLimiting("ContactSubmission");

        publicGroup.MapPost("/", async (HttpContext context, ISender sender, CreateContactSubmissionCommand command) =>
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var remoteIp = !string.IsNullOrEmpty(forwardedFor) 
                ? forwardedFor.Split(',').FirstOrDefault()?.Trim() 
                : context.Connection.RemoteIpAddress?.ToString();

            command.RemoteIp = remoteIp;

            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Created($"/api/v1/admin/contact-submissions/{result.Value}", result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        var adminGroup = app.MapGroup("/api/v1/admin/contact-submissions")
            .WithTags("Contact Submissions (Admin)")
            .RequireAuthorization();

        adminGroup.MapGet("/", async ([AsParameters] GetAdminContactSubmissionsQuery query, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(query);
                if (!result.IsSuccess)
                {
                    return Results.BadRequest(result.ToApiResponse());
                }

                var apiResponse = ApiResponse<IEnumerable<ContactSubmissionDto>>.Success(result.Value!.Items, new PaginationMeta(query.Page.GetValueOrDefault(1), query.PageSize.GetValueOrDefault(20), result.Value.TotalCount));

                return Results.Ok(apiResponse);
            }
            catch (Exception ex)
            {
                return Results.InternalServerError(ex.ToString());
            }
        });

        adminGroup.MapPatch("/{id:guid}/mark-read", async (Guid id, ISender sender) =>
        {
            var command = new MarkContactSubmissionReadCommand { Id = id };
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        adminGroup.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteContactSubmissionCommand { Id = id };
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.ToApiResponse());
        });
    }
}
