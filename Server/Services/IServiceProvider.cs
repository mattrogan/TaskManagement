namespace TaskManagement.Server.Services
{
    public interface IServiceProvider<T> where T : class
    {
        Task<IEnumerable<T>> GetAsync();
        Task<T?> GetSingleAsync(int id);
        Task<bool> DeleteAsync(T task);
        Task<bool> AddAsync(T item);
        Task<bool> UpdateAsync(T task);
    }
}
