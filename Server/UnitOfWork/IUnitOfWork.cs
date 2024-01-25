namespace Server.UnitOfWork;

public interface IUnitOfWork
{
    IRepository<T> GetRepository<T>() where T : class;
}