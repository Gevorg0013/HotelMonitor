using HotelMonitor.Api.Models;

namespace HotelMonitor.SignallingHub;

public interface IMonitorHub
{
    Task NewMonitorImages(IReadOnlyCollection<MonitorImageNotification> images);
}
