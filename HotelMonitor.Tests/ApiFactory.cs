using System.Net.Http.Json;
using HotelMonitor.Api;
using HotelMonitor.Core.DataLayer;
using HotelMonitor.Core.DataLayer.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HotelMonitor.Tests;

/// <summary>
/// Runs the real API in memory against a private in-memory SQLite database,
/// so tests never touch VTM.Db.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Username = "tester";
    public const string Password = "Test@12345";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public FailingSaveInterceptor SaveFailure { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // Tests must not depend on the git-ignored appsettings.Development.json.
        builder.UseSetting("Jwt:SigningKey", "test-only-signing-key-0123456789-abcdefghij");
        builder.UseSetting("ConnectionStrings:VTMConnStr", "unused-tests-run-on-sqlite");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<HotelMonitorDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<HotelMonitorDbContext>>();
            services.AddDbContext<HotelMonitorDbContext>(options => options
                .UseSqlite(_connection)
                .AddInterceptors(SaveFailure));
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await WithDb(async db =>
        {
            await db.Database.EnsureCreatedAsync();
            var user = new AuthUser { Username = Username, IsActive = true, CreatedAt = DateTime.UtcNow };
            user.PasswordHash = new PasswordHasher<AuthUser>().HashPassword(user, Password);
            db.AuthUsers.Add(user);
            await db.SaveChangesAsync();
            return 0;
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }

    public async Task<T> WithDb<T>(Func<HotelMonitorDbContext, Task<T>> work)
    {
        await using var scope = Services.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<HotelMonitorDbContext>());
    }

    public async Task<string> GetTokenAsync()
    {
        var response = await CreateClient().PostAsJsonAsync("/api/monitor/auth/login", new { username = Username, password = Password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    public async Task<HttpClient> AuthorizedClientAsync()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", await GetTokenAsync());
        return client;
    }

    public async Task<HubConnection> ConnectHubAsync()
    {
        var token = await GetTokenAsync();
        var hub = new HubConnectionBuilder()
            .WithUrl(new Uri(Server.BaseAddress, "/hubs/monitor"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .Build();
        await hub.StartAsync();
        return hub;
    }
}

/// <summary>
/// Makes SaveChanges fail on demand AFTER its INSERTs have run inside the upload transaction,
/// so only a rollback can remove them; proves the save is all-or-nothing and sends no notification.
/// </summary>
public sealed class FailingSaveInterceptor : SaveChangesInterceptor
{
    public bool Enabled { get; set; }

    public int RowsWrittenBeforeFailure { get; private set; }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (!Enabled)
            return base.SavedChangesAsync(eventData, result, cancellationToken);

        RowsWrittenBeforeFailure = result;
        throw new DbUpdateException("Simulated database failure.");
    }
}
