namespace HotelMonitor.Api.Models;

public sealed record MonitorLoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
