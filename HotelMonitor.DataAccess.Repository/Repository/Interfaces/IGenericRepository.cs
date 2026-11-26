using System.Linq.Expressions;

namespace HotelMonitor.DataAccess.Repository;

public interface IGenericRepository<TSource> where TSource : class
{
    Task<TSource?> GetFirstOrDefaultAsync(Expression<Func<TSource, bool>> queryConditions);

    Task<TResult?> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TSource, bool>> queryConditions,
        Expression<Func<TSource, TResult>> querySelector);

    Task<ICollection<TResult>> GetAllAsync<TResult, TKey>(
        Expression<Func<TSource, TResult>> querySelector,
        Expression<Func<TSource, TKey>> orderBy);

    Task<bool> AnyAsync(Expression<Func<TSource, bool>> queryConditions);

    Task AddAsync(TSource source);

    void Update(TSource source);

    void Remove(TSource source);
}
