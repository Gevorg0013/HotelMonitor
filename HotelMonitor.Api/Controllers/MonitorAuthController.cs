using HotelMonitor.Api.Models;
using HotelMonitor.Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelMonitor.Api.Controllers;

[ApiController]
[Route("api/monitor/auth")]
public class MonitorAuthController : ControllerBase
{
    private readonly IMonitorAuthService _monitorAuthService;

    public MonitorAuthController(IMonitorAuthService monitorAuthService)
    {
        _monitorAuthService = monitorAuthService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(MonitorLoginRequest model)
    {
        var result = await _monitorAuthService.LoginAsync(model);
        return StatusCode(result.Item1.StatusCode, result.Item2);
    }
}
