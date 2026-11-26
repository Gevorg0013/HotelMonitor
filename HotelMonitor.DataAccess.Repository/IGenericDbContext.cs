using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HotelMonitor.DataAccess.Repository;

public interface IGenericDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
