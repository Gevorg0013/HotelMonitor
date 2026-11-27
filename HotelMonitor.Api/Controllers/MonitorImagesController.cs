using HotelMonitor.Api.Models;
using HotelMonitor.Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace HotelMonitor.Api.Controllers;

[ApiController]
[Route("api/monitor/images")]
public class MonitorImagesController : ControllerBase
{
    private readonly IMonitorImageService _monitorImageService;
    private const long MaxUploadBytes = 100 * 1024 * 1024;

    public MonitorImagesController(IMonitorImageService monitorImageService)
    {
        _monitorImageService = monitorImageService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> Upload([FromForm] ImageUploadRequest model)
    {
        var result = await _monitorImageService.UploadAsync(model);
        return result.Item2 is null
            ? StatusCode(result.Item1.StatusCode, result.Item1.Message)
            : StatusCode(result.Item1.StatusCode, result.Item2);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _monitorImageService.GetAllAsync();
        return StatusCode(result.Item1.StatusCode, result.Item2);
    }
}
