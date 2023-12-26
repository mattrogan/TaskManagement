using Microsoft.EntityFrameworkCore;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Data
{
    public class TaskContext : DbContext, ITaskContext
    {
        public TaskContext(DbContextOptions<TaskContext> opts)
            : base(opts)
        {

        }
        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source = TaskManagement.db");
        }

        public async Task SaveChangesAsync()
            => await base.SaveChangesAsync();

        public async Task AddAsync(TodoItem item)
            => await base.AddAsync(item);

        public void Remove(TodoItem task)
            => base.Remove(task);
    }
}
