using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.DataAccess.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelMonitor.Core.DataLayer.Repository;

public interface IHotelMonitorUnitOfWork
{
    IGenericRepository<AuthUser> AuthUserRepository { get; }

    IGenericRepository<MonitorImage> MonitorImageRepository { get; }

    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Disposing the transaction without committing it rolls it back.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Forgets pending changes, so the context stays usable after a failed save.</summary>
    void DiscardChanges();
}
