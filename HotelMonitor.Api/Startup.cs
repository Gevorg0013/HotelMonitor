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
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        services.AddScoped<IPasswordHasher<MonitorIdentityAuth>, PasswordHasher<MonitorIdentityAuth>>();
        services.AddSingleton<IMonitorTokenService, MonitorTokenService>();
        services.AddSingleton<IMonitorImageNotifier, MonitorImageNotifier>();
        services.AddScoped<IMonitorAuthService, MonitorAuthService>();
        services.AddScoped<IMonitorImageService, MonitorImageService>();

        services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        services.AddCors();
        services.AddControllers();
        services.AddSignalR();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(bearer =>
                {
                    bearer.MapInboundClaims = false;

                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,

                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.SigningKey)),

                        NameClaimType = MonitorClaims.Username
                    };
                });

        services.AddAuthorization();
    }

    public void Configure(WebApplication app)
    {
        var uploadDirectory = MonitorImageService.GetUploadDirectory(Configuration, app.Environment);
        Directory.CreateDirectory(uploadDirectory);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadDirectory),
            RequestPath = MonitorImageService.UploadRequestPath
        });

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true)
        .AllowCredentials());
        app.MapHub<MonitorHub>(MonitorHub.Path);
    }
}