using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using MediaPortfolio.Infrastructure.Persistence;

namespace MediaPortfolio.API.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that starts a real PostgreSQL container and
/// wires it into the app configuration BEFORE the host is built.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .Build();

    public Moq.Mock<MediaPortfolio.Application.Interfaces.ITurnstileVerificationService> TurnstileMock { get; } = new();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override configuration — called before host is built
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _dbContainer.GetConnectionString(),
                ["Jwt:SigningKey"] = "TestSecretKey12345678TestSecretKey12345678",
                ["Jwt:AccessTokenLifetimeMinutes"] = "60",
                ["AdminSeed:InitialPassword"] = "TestPassw0rd!",
                ["Hangfire:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // ConfigureServices runs AFTER Program.cs has registered services.
            // The Hangfire:Enabled=false config flag stops the job from being scheduled (Program.cs),
            // but AddHangfireServer() was already called in InfrastructureServiceExtensions, so the
            // background server is still registered. Remove ALL Hangfire-related service descriptors
            // to prevent the server from starting during tests. This is test-scope only — it has no
            // effect on Development, Docker, or Production.
            var hangfireDescs = services
                .Where(d =>
                    (d.ServiceType.Namespace?.Contains("Hangfire") == true) ||
                    (d.ImplementationType?.Namespace?.Contains("Hangfire") == true) ||
                    (d.ServiceType == typeof(IHostedService) &&
                     (d.ImplementationType?.FullName?.Contains("Hangfire") == true ||
                      d.ImplementationFactory?.Method.DeclaringType?.FullName?.Contains("Hangfire") == true)))
                .ToList();
            foreach (var svc in hangfireDescs)
                services.Remove(svc);

            // Remove the real HTTP-based Turnstile service and replace with mock
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MediaPortfolio.Application.Interfaces.ITurnstileVerificationService));
            if (descriptor != null) services.Remove(descriptor);
            services.AddSingleton(TurnstileMock.Object);

            // Run migrations after the service provider is built
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MediaPortfolioDbContext>();
            db.Database.Migrate();
        });
    }
}
