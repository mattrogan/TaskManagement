using System.Linq.Expressions;

namespace Server.UnitOfWork;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T?> SingleAsync(int id);
    Task<T> AddAsync(T entry);
    Task<bool> UpdateAsync(T entry);
    Task<bool> DeleteAsync(T entry);
    Task<IQueryable<TResult>> QueryAsync<TResult>(Expression<Func<T, bool>> expression, Expression<Func<T, TResult>> selector) where TResult : class;
}
