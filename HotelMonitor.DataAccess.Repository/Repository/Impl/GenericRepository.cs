using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace HotelMonitor.DataAccess.Repository;

public class GenericRepository<TSource>(IGenericDbContext context) : IGenericRepository<TSource> where TSource : class
{
    private readonly DbSet<TSource> _dbSet = context.Set<TSource>();

    public Task<TSource?> GetFirstOrDefaultAsync(Expression<Func<TSource, bool>> queryConditions) =>
        _dbSet.FirstOrDefaultAsync(queryConditions);

    public Task<TResult?> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TSource, bool>> queryConditions,
        Expression<Func<TSource, TResult>> querySelector) =>
        _dbSet.AsNoTracking().Where(queryConditions).Select(querySelector).FirstOrDefaultAsync();

    public async Task<ICollection<TResult>> GetAllAsync<TResult, TKey>(
        Expression<Func<TSource, TResult>> querySelector,
        Expression<Func<TSource, TKey>> orderBy) =>
        await _dbSet.AsNoTracking().OrderBy(orderBy).Select(querySelector).ToListAsync();

    public Task<bool> AnyAsync(Expression<Func<TSource, bool>> queryConditions) => _dbSet.AnyAsync(queryConditions);

    public async Task AddAsync(TSource source) => await _dbSet.AddAsync(source);

    public void Update(TSource source)
    {
        _dbSet.Attach(source);
        context.Entry(source).State = EntityState.Modified;
    }

    public void Remove(TSource source) => _dbSet.Remove(source);
}
