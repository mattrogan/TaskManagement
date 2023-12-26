using TaskManagement.Shared.Models;

namespace TaskManagement.Server.SeedData
{
    public static class SeedData_InitialTasks
    {
        public static List<TodoItem> InitialTasks => new()
        {
            new TodoItem
            {
                Id = 1,
                Title = "Clean the dishes",
                Description = "Wash all the dirty dishes, dry them, and put them away",
                DueDate = DateTime.Now.AddDays(7)
            },

            new TodoItem
            {
                Id = 2,
                Title = "Wash dirty clothes",
                Description = "Put all dirty clothes in the washing machine",
                DueDate = DateTime.Now.AddDays(7)
            }
        };
    }
}
