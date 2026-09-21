using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MediaPortfolio.Application.Exceptions;
using MediaPortfolio.Domain.Exceptions;
using MediaPortfolio.Application.Common;
using MediaPortfolio.API.Models;
using System.Linq;

namespace MediaPortfolio.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = ApiResponse<object>.Failure("INTERNAL_ERROR", "An unexpected error occurred.");
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var apiErrors = validationEx.Errors
                .SelectMany(kvp => kvp.Value.Select(err => new ApiError("FIELD_VALIDATION_FAILED", kvp.Key, err)))
                .ToList();
                
            response = ApiResponse<object>.Failure(apiErrors);
        }
        else if (exception is NotFoundException notFoundEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            response = ApiResponse<object>.Failure(notFoundEx.Code, notFoundEx.Message);
        }
        else if (exception is DomainException domainEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            response = ApiResponse<object>.Failure("DOMAIN_ERROR", domainEx.Message);
        }

        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(result);
    }
}
