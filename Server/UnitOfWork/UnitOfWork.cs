using TaskManagement.Server.Data;

namespace Server.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TaskContext context;
    private Dictionary<Type, object> repositories;

    public UnitOfWork(TaskContext context)
    {
        this.context = context;
        this.repositories = new();
    }

    public void Commit()
        => context.SaveChanges();

    public void Rollback()
    {

    }

    public IRepository<T> GetRepository<T>() where T : class
    {
        if (repositories.ContainsKey(typeof(T)))
            return (IRepository<T>)repositories[typeof(T)];

        var repository = new Repository<T>(context);
        repositories.Add(typeof(T), repository);
        return repository;
    }

    public void Dispose()
        => context.Dispose();
}