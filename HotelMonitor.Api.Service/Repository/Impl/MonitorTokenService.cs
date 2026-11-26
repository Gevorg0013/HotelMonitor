using System.Security.Claims;
using System.Text;
using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.Models.Configuration;
using HotelMonitor.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HotelMonitor.Api.Service;

public sealed class MonitorTokenService(IOptions<JwtOptions> options) : IMonitorTokenService
{
    public (string Token, int ExpiresIn) Create(AuthUser monitor)
    {
        var jwt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = DateTime.UtcNow.AddSeconds(jwt.ExpiresInSeconds),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity([new Claim(MonitorClaims.Username, monitor.Username)])
        });

        return (token, jwt.ExpiresInSeconds);
    }
}
