using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ApprovalWorkflow.Infrastructure.Persistence;
using System.Security.Cryptography.X509Certificates;


namespace ApprovalWorkflow.Tests.Infrastructure;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;

    public WebAppFactory()
    {
        _pgContainer = new PostgreSqlBuilder()
        .WithDatabase("Testing")
        .WithPassword("postgres")
        .Build();
    }

    public async Task InitializeAsync()
    {
        await _pgContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(service =>
        {
            var validDb = service.SingleOrDefault(
                t => t.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );

            if(validDb != null)
            {
                service.Remove(validDb);
            }

            service.AddDbContext<AppDbContext>(option =>
            {
                option.UseNpgsql(_pgContainer.GetConnectionString());
            });

            var serviceProvider = service.BuildServiceProvider();

            var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.Migrate();
        });
    }

    public async Task DisposeAsync()
    {
        await _pgContainer.DisposeAsync();
    }
}
