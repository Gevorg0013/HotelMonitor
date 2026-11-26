using HotelMonitor.Api.Models;
using HotelMonitor.Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelMonitor.Api.Controllers;

[ApiController]
[Route("api/monitor/images")]
public class MonitorImagesController : ControllerBase
{
    private readonly IMonitorImageService _monitorImageService;

    public MonitorImagesController(IMonitorImageService monitorImageService)
    {
        _monitorImageService = monitorImageService;
    }
    [AllowAnonymous]
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

    // Several images of up to 10 MB each in one request.
    private const long MaxUploadBytes = 100 * 1024 * 1024;


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _monitorImageService.GetAllAsync();
        return StatusCode(result.Item1.StatusCode, result.Item2);
    }

    [HttpGet("{id:int}/content")]
    public async Task<IActionResult> GetContent(int id)
    {
        var result = await _monitorImageService.GetContentAsync(id);
        if (result.Item2 is null)
            return StatusCode(result.Item1.StatusCode);

        Response.Headers.XContentTypeOptions = "nosniff";
        return File(result.Item2.ImageData, result.Item2.ContentType);
    }
}
