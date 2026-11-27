using HotelMonitor.Api.Models;
using HotelMonitor.Api.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HotelMonitor.SignallingHub;

public sealed class MonitorImageNotifier(IHubContext<MonitorHub, IMonitorHub> hub, ILogger<MonitorImageNotifier> logger)
    : IMonitorImageNotifier
{
    public async Task NotifyAsync(IReadOnlyCollection<MonitorImageNotification> images)
    {
        try
        {
            await hub.Clients.All.OnSendImage(images);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast {Count} uploaded images", images.Count);
        }
    }
}
