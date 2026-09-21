using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using MediaPortfolio.Application.Features.Identity.Commands.LoginAdmin;
using MediaPortfolio.Application.Features.Identity.Commands.RefreshToken;
using MediaPortfolio.Application.Features.Identity.Commands.LogoutAdmin;
using MediaPortfolio.Application.Features.Identity.Commands.ForgotPassword;
using MediaPortfolio.Application.Features.Identity.Commands.ResetPassword;
using MediaPortfolio.Application.Features.Identity.Commands.ChangePassword;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MediaPortfolio.API.Extensions;

namespace MediaPortfolio.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/login", async (ISender sender, LoginAdminCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        }).RequireRateLimiting("AuthLogin");

        group.MapPost("/refresh", async (ISender sender, RefreshTokenCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        group.MapPost("/logout", async (ISender sender, LogoutAdminCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        }).RequireAuthorization(); // Contract: Admin-only; must supply valid Bearer + refresh token

        group.MapPost("/forgot-password", async (ISender sender, ForgotPasswordCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        group.MapPost("/reset-password", async (ISender sender, ResetPasswordCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        });

        group.MapPost("/change-password", async (ISender sender, ChangePasswordCommand command) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.ToApiResponse()) : Results.BadRequest(result.ToApiResponse());
        }).RequireAuthorization(); // Requires JWT
    }
}
