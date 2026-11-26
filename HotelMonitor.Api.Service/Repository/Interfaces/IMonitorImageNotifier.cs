using HotelMonitor.Api.Models;

namespace HotelMonitor.Api.Service;

public interface IMonitorImageNotifier
{
    /// <summary>Sends one notification for a whole upload.</summary>
    Task NotifyAsync(IReadOnlyCollection<MonitorImageNotification> images);
}
