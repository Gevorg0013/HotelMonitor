using System.ComponentModel.DataAnnotations;

namespace HotelMonitor.Api.Models;

public sealed class MonitorLoginRequest
{
    [Required, MaxLength(100)] public string Username { get; set; } = "";
    [Required, MaxLength(128)] public string Password { get; set; } = "";
}
