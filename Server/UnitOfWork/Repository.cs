using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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
        => await context.Set<T>().Where(condition).ToListAsync();

    public async Task<T?> SingleAsync(int id)
        => await context.Set<T>().FindAsync(id);

    public async Task<T> AddAsync(T entry)
    {
        await context.AddAsync(entry);
        await context.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> UpdateAsync(T entry)
    {
        try
        {
            context.Update(entry);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(T entry)
    {
        try
        {
            context.Remove(entry);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
