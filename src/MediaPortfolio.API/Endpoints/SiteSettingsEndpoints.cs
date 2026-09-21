using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Settings.Queries.GetSiteSettings;
using MediaPortfolio.Application.Features.Settings.Commands.UpdateSiteSettings;
using MediaPortfolio.API.Extensions;

namespace MediaPortfolio.API.Endpoints;

public static class SiteSettingsEndpoints
{
    public static void MapSiteSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/v1/site-settings")
            .WithTags("Site Settings (Public)");

        publicGroup.MapGet("/", async (HttpContext context, ISender sender) =>
        {
            var language = context.Request.Headers["Accept-Language"].ToString();
            var query = new GetSiteSettingsQuery { Language = string.IsNullOrWhiteSpace(language) ? "ar" : language };
            
            var result = await sender.Send(query);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        var adminGroup = app.MapGroup("/api/v1/admin/site-settings")
            .WithTags("Site Settings (Admin)")
            .RequireAuthorization();

        adminGroup.MapPut("/", async (ISender sender, UpdateSiteSettingsCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });
    }
}
