namespace HotelMonitor.Api.Models;

public sealed record MonitorLoginResponse(string AccessToken, string TokenType, int ExpiresIn);
