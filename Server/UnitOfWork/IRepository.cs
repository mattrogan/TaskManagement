using System.Linq.Expressions;

namespace Server.UnitOfWork;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T?> SingleAsync(int id);
}