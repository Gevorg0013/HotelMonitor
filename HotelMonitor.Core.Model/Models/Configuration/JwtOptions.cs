using System.ComponentModel.DataAnnotations;

namespace HotelMonitor.Models.Configuration;

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    [Required] public string Issuer { get; set; } = "";
    [Required] public string Audience { get; set; } = "";

    /// <summary>HMAC-SHA256 key; must be at least 32 characters. Supply via environment or user secrets.</summary>
    [Required, MinLength(32)] public string SigningKey { get; set; } = "";

    [Range(60, 86400)] public int ExpiresInSeconds { get; set; } = 3600;
}
