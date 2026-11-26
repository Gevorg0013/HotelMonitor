using HotelMonitor.Api.Models;
using HotelMonitor.Common.Models;
using HotelMonitor.Core.Service;

namespace HotelMonitor.Api.Service;

public interface IMonitorAuthService : IBaseCoreService
{
    /// <summary>Unauthorized when the credentials are invalid or the monitor is inactive.</summary>
    Task<(BaseResponse, MonitorLoginResponse?)> LoginAsync(MonitorLoginRequest model);
}
