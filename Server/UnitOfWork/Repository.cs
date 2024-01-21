using System.Linq.Expressions;
using TaskManagement.Server.Data;

namespace Server.UnitOfWork;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly TaskContext context;
    
    public Repository(TaskContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> condition)
        => context.Set<T>().Where(condition);

    public async Task<T?> SingleAsync(int id)
        => await context.Set<T>().FindAsync(id);
}
