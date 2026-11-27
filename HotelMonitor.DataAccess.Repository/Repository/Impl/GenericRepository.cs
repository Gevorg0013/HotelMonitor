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

    public async Task<ICollection<TSource>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public Task<bool> AnyAsync(Expression<Func<TSource, bool>> queryConditions) => _dbSet.AnyAsync(queryConditions);

    public async Task AddAsync(TSource source) => await _dbSet.AddAsync(source);

    public async Task AddRangeAsync(IEnumerable<TSource> sources)
    => await _dbSet.AddRangeAsync(sources);

    public void Update(TSource source)
    {
        _dbSet.Attach(source);
        context.Entry(source).State = EntityState.Modified;
    }

    public void Remove(TSource source) => _dbSet.Remove(source);
}
