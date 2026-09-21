using System.Text.Json.Serialization;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediaPortfolio.Application;
using MediaPortfolio.Infrastructure;
using MediaPortfolio.API.Middleware;
using MediaPortfolio.API.Endpoints;
using MediaPortfolio.Infrastructure.Persistence;
using MediaPortfolio.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Application & Infrastructure services
// ---------------------------------------------------------------------------
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ---------------------------------------------------------------------------
// JSON Serialization
// ---------------------------------------------------------------------------
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ---------------------------------------------------------------------------
// Authentication — JWT Bearer
// ---------------------------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,       // no issuer configured in spec
            ValidateAudience = false,     // no audience configured in spec
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!))
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// Health checks — EF Core DbContext probe (config-independent at startup time)
// ---------------------------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MediaPortfolioDbContext>();

// ---------------------------------------------------------------------------
// Rate Limiting
// ---------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        var response = MediaPortfolio.API.Models.ApiResponse<object>.Failure("RATE_LIMITED", "Too many requests in the current window.");
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            response, 
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }, 
            token);
    };

    options.AddPolicy("TrackingEndpoints", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromSeconds(10)
            }));

    options.AddPolicy("ContactSubmission", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(60)
            }));

    options.AddFixedWindowLimiter("AuthLogin", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(60);
        opt.QueueLimit = 0;
    });
});

// ---------------------------------------------------------------------------
// CORS
// ---------------------------------------------------------------------------
var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? builder.Configuration["Cors__AllowedOrigin"] ?? "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---------------------------------------------------------------------------
// Swagger / OpenAPI (Swashbuckle) — registered always, exposed only in Development
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sainin Media Portfolio API",
        Version = "v1",
        Description = "Admin API for the Sainin media portfolio backend."
    });

    // JWT Bearer "Authorize" button in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your JWT access token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ---------------------------------------------------------------------------
// Build host & Auto-Migrate / Seed
// ---------------------------------------------------------------------------
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediaPortfolioDbContext>();
    db.Database.Migrate();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var initialPassword = config["AdminSeed:InitialPassword"] ?? "P@ssw0rd123!";

    ApplicationDbContextSeed.SeedDefaultUserAsync(db, passwordHasher, initialPassword).Wait();
    ApplicationDbContextSeed.SeedSampleDataAsync(db).Wait();
}

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger UI — Development only, never staging/production
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sainin Media Portfolio API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// CORS must be before Auth and RateLimiter
app.UseCors("FrontendCorsPolicy");

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------------------
// Endpoint mappings
// ---------------------------------------------------------------------------
app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapSecondaryServiceEndpoints();
app.MapMediaUploadEndpoints();
app.MapVideoAdminEndpoints();
app.MapVideoPublicEndpoints();
app.MapContactEndpoints();
app.MapSiteSettingsEndpoints();
app.MapAnalyticsEndpoints();

var provider = app.Configuration["MediaStorage:Provider"];
if (string.Equals(provider, "Local", System.StringComparison.OrdinalIgnoreCase))
{
    app.MapLocalMediaEndpoints();
}

app.MapHealthChecks("/health");

var hangfireEnabled = app.Configuration.GetValue<bool>("Hangfire:Enabled", true);

if (app.Environment.IsDevelopment() && hangfireEnabled)
{
    app.UseHangfireDashboard("/hangfire");
}

if (hangfireEnabled)
{
    Hangfire.RecurringJob.AddOrUpdate<MediaPortfolio.Application.Interfaces.IContactRetentionService>(
        "retention-30d",
        svc => svc.DeleteOldReadSubmissionsAsync(default),
        Hangfire.Cron.Daily
    );
}

app.Run();

// Exposed for WebApplicationFactory in integration tests
public partial class Program { }
