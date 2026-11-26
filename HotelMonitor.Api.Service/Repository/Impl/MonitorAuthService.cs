using System.Net;
using HotelMonitor.Api.Models;
using HotelMonitor.Common.Models;
using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.Core.DataLayer.Repository;
using HotelMonitor.Core.Service;
using Microsoft.AspNetCore.Identity;

namespace HotelMonitor.Api.Service;

public sealed class MonitorAuthService(
    IHotelMonitorUnitOfWork unitOfWork,
    IPasswordHasher<AuthUser> hasher,
    IMonitorTokenService tokens)
    : BaseCoreService(unitOfWork), IMonitorAuthService
{
    public async Task<(BaseResponse, MonitorLoginResponse?)> LoginAsync(MonitorLoginRequest model)
    {
        var monitor = await _unitOfWork.AuthUserRepository.GetFirstOrDefaultAsync(x => x.Username == model.Username && x.IsActive);

        if (monitor is null || hasher.VerifyHashedPassword(monitor, monitor.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
            return (new BaseResponse(HttpStatusCode.Unauthorized, "Invalid username or password."), null);

        var (token, expiresIn) = tokens.Create(monitor);
        return (new BaseResponse(), new MonitorLoginResponse(token, "Bearer", expiresIn));
    }
}
