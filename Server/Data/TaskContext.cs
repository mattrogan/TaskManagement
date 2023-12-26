using Microsoft.EntityFrameworkCore;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Data
{
    public class TaskContext : DbContext
    {
        private readonly IConfiguration cfg;

        public TaskContext(IConfiguration cfg)
        {
            this.cfg = cfg;
        }

        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbName = cfg["DbName"];
            // Construct the full connection string
            string connectionString = string.Format(cfg.GetConnectionString("DefaultConnection"), GetDatabasePath(dbName));
            optionsBuilder.UseSqlite(connectionString);
        }

        private string GetDatabasePath(string dbName)
        {
            string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(specialFolder, $"{dbName}.db");
        }
    }
}
