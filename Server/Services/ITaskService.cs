using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TodoItem>> GetTasksAsync();
        Task<TodoItem?> GetTaskAsync(int id);
        Task<bool> DeleteTaskAsync(TodoItem task);
        Task<bool> AddTaskAsync(TodoItem item);
        Task<bool> UpdateTaskAsync(TodoItem task);
    }
}
