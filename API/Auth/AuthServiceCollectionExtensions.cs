using System.Text;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Auth;

public static class AuthServiceCollectionExtensions
{
    public const string GoogleSection = "Authentication:Google";
    public const string GitHubSection = "Authentication:GitHub";
    public const string SmartScheme = "Smart";
    public const string BearerPrefix = "Bearer ";

    public static IServiceCollection AddApplicationAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<AgoraRivasContext>((serviceProvider, options) =>
        {
            string? connectionString = serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddIdentity<Profile, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<AgoraRivasContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        var authentication = services.AddAuthentication(options =>
            {
                options.DefaultScheme = SmartScheme;
                options.DefaultAuthenticateScheme = SmartScheme;
                options.DefaultChallengeScheme = SmartScheme;
                options.DefaultForbidScheme = SmartScheme;
            })
            .AddPolicyScheme(SmartScheme, SmartScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.Authorization.ToString().StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : IdentityConstants.ApplicationScheme;
            })
            .AddJwtBearer(options => options.MapInboundClaims = true);

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwt) =>
            {
                var jwtOptions = jwt.Value;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        AddExternalProviders(authentication, configuration);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ReturnUrlValidator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IExternalLoginService, ExternalLoginService>();

        return services;
    }

    private static void AddExternalProviders(AuthenticationBuilder authentication, IConfiguration configuration)
    {
        var google = ReadProvider(configuration, GoogleSection);
        if (google.IsConfigured)
        {
            authentication.AddGoogle(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = google.ClientId!;
                options.ClientSecret = google.ClientSecret!;
                options.SaveTokens = false;
                options.ClaimActions.MapJsonKey(ExternalClaimTypes.EmailVerified, "email_verified");
            });
        }

        var gitHub = ReadProvider(configuration, GitHubSection);
        if (gitHub.IsConfigured)
        {
            authentication.AddGitHub(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = gitHub.ClientId!;
                options.ClientSecret = gitHub.ClientSecret!;
                options.SaveTokens = false;
                GitHubEmailClaims.Configure(options);
            });
        }
    }

    private static ExternalProviderOptions ReadProvider(IConfiguration configuration, string section)
    {
        var options = configuration.GetSection(section).Get<ExternalProviderOptions>() ?? new ExternalProviderOptions();
        if (options.IsPartiallyConfigured)
        {
            throw new InvalidOperationException($"{section} requires both ClientId and ClientSecret.");
        }

        return options;
    }
}
