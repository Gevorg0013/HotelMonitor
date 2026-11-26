using HotelMonitor.Api.Models;
using HotelMonitor.Common.Models;
using HotelMonitor.Core.Service;

namespace HotelMonitor.Api.Service;

public interface IMonitorImageService : IBaseCoreService
{
    /// <summary>Saves all files in one transaction, then sends a single SignalR notification.</summary>
    Task<(BaseResponse, ICollection<MonitorImageResponse>?)> UploadAsync(ImageUploadRequest model);

    Task<(BaseResponse, ICollection<MonitorImageResponse>)> GetAllAsync();

    Task<(BaseResponse, ImageContentResponse?)> GetContentAsync(int id);
}
