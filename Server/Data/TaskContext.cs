using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Data.ModelConfigurations;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Data
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> opts)
            : base(opts)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TodoItemConfiguration());
        }

        public async Task SaveChangesAsync()
            => await base.SaveChangesAsync();

        public async Task AddAsync(TodoItem item)
            => await base.AddAsync(item);

        public void Remove(TodoItem task)
            => base.Remove(task);
    }
}
