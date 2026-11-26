using System.Text;
using HotelMonitor.Api.Service;
using HotelMonitor.Core.DataLayer;
using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.Core.DataLayer.Repository;
using HotelMonitor.Models.Configuration;
using HotelMonitor.Security;
using HotelMonitor.SignallingHub;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HotelMonitor.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
        => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var jwt = Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
        services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.Section).ValidateDataAnnotations().ValidateOnStart();

        services.AddDbContext<HotelMonitorDbContext>(options => options.UseSqlServer(
            Configuration.GetConnectionString("VTMConnStr"),
            sql => sql.MigrationsHistoryTable("__HotelMonitorMigrationsHistory")));

        services.AddScoped<IHotelMonitorUnitOfWork, HotelMonitorUnitOfWork>();
        services.AddScoped<IPasswordHasher<AuthUser>, PasswordHasher<AuthUser>>();
        services.AddSingleton<IMonitorTokenService, MonitorTokenService>();
        services.AddSingleton<IMonitorImageNotifier, MonitorImageNotifier>();
        services.AddScoped<IMonitorAuthService, MonitorAuthService>();
        services.AddScoped<IMonitorImageService, MonitorImageService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(bearer =>
        {
            bearer.MapInboundClaims = false;
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                NameClaimType = MonitorClaims.Username
            };
            bearer.Events = new JwtBearerEvents
            {
                // Browsers cannot set headers on WebSocket requests; accept the query token for the hub only.
                OnMessageReceived = context =>
                {
                    string? token = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(token) && context.Request.Path.StartsWithSegments(MonitorHub.Path))
                        context.Token = token;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddCors(cors => cors.AddDefaultPolicy(p => p
            .WithOrigins(Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

        services.AddControllers();
        services.AddSignalR();
    }

    public void Configure(WebApplication app)
    {
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<MonitorHub>(MonitorHub.Path);
    }
}
