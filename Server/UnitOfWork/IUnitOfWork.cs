namespace Server.UnitOfWork;

public interface IUnitOfWork
{
    void Commit();
    void Rollback();
    IRepository<T> GetRepository<T>() where T : class;
}