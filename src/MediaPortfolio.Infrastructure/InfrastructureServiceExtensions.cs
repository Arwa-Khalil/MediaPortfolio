using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Infrastructure.Persistence;
using MediaPortfolio.Infrastructure.Persistence.Interceptors;
using MediaPortfolio.Infrastructure.Identity;
using MediaPortfolio.Application.Features.Identity.Commands.LoginAdmin;
using Microsoft.AspNetCore.Identity;
using MediaPortfolio.Domain.Entities;
using Hangfire;
using Hangfire.PostgreSql;

namespace MediaPortfolio.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<MediaPortfolioDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
            options.AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<MediaPortfolioDbContext>());
        
        services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddTransient<IEmailSender, Services.LoggingEmailSender>();

        var provider = configuration["MediaStorage:Provider"];
        if (string.Equals(provider, "Cloudflare", System.StringComparison.OrdinalIgnoreCase))
        {
            var streamAccountId = configuration["Cloudflare:StreamAccountId"];
            var streamApiToken = configuration["Cloudflare:StreamApiToken"];
            var imagesAccountId = configuration["Cloudflare:ImagesAccountId"];
            var imagesApiToken = configuration["Cloudflare:ImagesApiToken"];
            
            if (string.IsNullOrEmpty(streamAccountId) || string.IsNullOrEmpty(streamApiToken) || 
                string.IsNullOrEmpty(imagesAccountId) || string.IsNullOrEmpty(imagesApiToken))
            {
                throw new System.InvalidOperationException("Cloudflare credentials are missing but provider is set to Cloudflare.");
            }

            services.AddHttpClient<IVideoStorageService, Services.CloudflareVideoStorageService>();
            services.AddHttpClient<IImageStorageService, Services.CloudflareImageStorageService>();
            services.AddTransient<IThumbnailUrlResolver, Services.CloudflareThumbnailUrlResolver>();
        }
        else
        {
            services.AddHttpContextAccessor();
            services.AddTransient<IVideoStorageService, Services.LocalVideoStorageService>();
            services.AddTransient<IImageStorageService, Services.LocalImageStorageService>();
            services.AddTransient<IThumbnailUrlResolver, Services.LocalThumbnailUrlResolver>();
        }
        
        services.AddHttpClient<ITurnstileVerificationService, Services.TurnstileVerificationService>();
        services.AddScoped<IContactRetentionService, Services.ContactRetentionService>();

        if (configuration.GetValue<bool>("Hangfire:Enabled", true))
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => 
                    options.UseNpgsqlConnection(configuration.GetConnectionString("Default"))
                ));

            services.AddHangfireServer();
        }
        
        return services;
    }
}
