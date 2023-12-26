using Microsoft.EntityFrameworkCore;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Data
{
    public interface ITaskContext
    {
        DbSet<TodoItem> TodoItems { get; }
        Task SaveChangesAsync();
        Task AddAsync(TodoItem item);
        void Remove(TodoItem task);
    }
}
