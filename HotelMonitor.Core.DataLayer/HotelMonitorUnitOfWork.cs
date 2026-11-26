using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.DataAccess.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace HotelMonitor.Core.DataLayer.Repository;

public class HotelMonitorUnitOfWork(HotelMonitorDbContext context) : IHotelMonitorUnitOfWork
{
    private IGenericRepository<AuthUser>? _authUserRepository;
    private IGenericRepository<MonitorImage>? _monitorImageRepository;

    public IGenericRepository<AuthUser> AuthUserRepository => _authUserRepository ??= new GenericRepository<AuthUser>(context);

    public IGenericRepository<MonitorImage> MonitorImageRepository => _monitorImageRepository ??= new GenericRepository<MonitorImage>(context);

    public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        context.Database.BeginTransactionAsync(ct);

    public void DiscardChanges() => context.ChangeTracker.Clear();
}
