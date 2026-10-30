using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Tests;

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    public const string AllowedOrigin = "https://app.example.test";

    private readonly string databaseName = Guid.NewGuid().ToString();

    public HttpClient CreateSecureClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = true
    });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=unused;Database=unused");
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-that-is-at-least-32-chars-long");
        builder.UseSetting("Authentication:Google:ClientId", "google-client-id");
        builder.UseSetting("Authentication:Google:ClientSecret", "google-client-secret");
        builder.UseSetting("Authentication:AllowedReturnOrigins:0", AllowedOrigin);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AgoraRivasContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AgoraRivasContext>>();
            services.AddDbContext<AgoraRivasContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }
}
