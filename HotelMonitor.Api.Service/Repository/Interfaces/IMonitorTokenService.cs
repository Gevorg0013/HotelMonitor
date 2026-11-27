using HotelMonitor.Core.DataLayer.Entities;

namespace HotelMonitor.Api.Service;

public interface IMonitorTokenService
{
    (string Token, int ExpiresIn) Create(MonitorIdentityAuth monitor);
}
