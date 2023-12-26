using Microsoft.EntityFrameworkCore;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Data
{
    public class TaskContext : DbContext
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
    }
}
