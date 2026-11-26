using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HotelMonitor.SignallingHub;

/// <summary>Server-to-client only: every connected monitor receives every new image.</summary>
[Authorize]
public sealed class MonitorHub : Hub<IMonitorHub>
{
    public const string Path = "/hubs/monitor";
}
