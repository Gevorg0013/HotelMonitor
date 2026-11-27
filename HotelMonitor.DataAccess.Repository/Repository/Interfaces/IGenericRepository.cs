using System.Linq.Expressions;

namespace HotelMonitor.DataAccess.Repository;

public interface IGenericRepository<TSource> where TSource : class
{
    Task<TSource?> GetFirstOrDefaultAsync(Expression<Func<TSource, bool>> queryConditions);

    Task<TResult?> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TSource, bool>> queryConditions,
        Expression<Func<TSource, TResult>> querySelector);

    Task<ICollection<TSource>> GetAllAsync();

    Task<bool> AnyAsync(Expression<Func<TSource, bool>> queryConditions);

    Task AddAsync(TSource source);

    Task AddRangeAsync(IEnumerable<TSource> sources);

    void Update(TSource source);

    void Remove(TSource source);
}
