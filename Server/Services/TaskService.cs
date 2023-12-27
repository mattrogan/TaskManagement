using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Data;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskContext ctx;

        public TaskService(ITaskContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<IEnumerable<TodoItem>> GetTasksAsync()
            => await ctx.TodoItems.ToListAsync();

        public async Task<TodoItem?> GetTaskAsync(int id)
            => await ctx.TodoItems.SingleOrDefaultAsync(t => t.Id == id);

        public async Task<bool> AddTaskAsync(TodoItem item)
        {
            try
            {
                await ctx.AddAsync(item);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(TodoItem task)
        {
            try
            {
                ctx.TodoItems.Update(task);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> DeleteTaskAsync(TodoItem task)
        {
            try
            {
                ctx.Remove(task);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
