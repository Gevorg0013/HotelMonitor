using HotelMonitor.Api.Models;

namespace HotelMonitor.SignallingHub;

public interface IMonitorHub
{
    Task OnSendImage(IReadOnlyCollection<MonitorImageNotification> images);
}
