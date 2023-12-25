using Microsoft.EntityFrameworkCore.Migrations;
using TaskManagement.Server.SeedData;

#nullable disable

namespace TaskManagement.Server.Migrations
{
    public partial class SeedInitialTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var task in SeedData_InitialTasks.InitialTasks)
            {
                migrationBuilder.InsertData(
                    table: "TodoItems",
                    columns: new[] { "Id", "Title", "Description", "DueDate" },
                    values: new object[] { task.Id, task.Title, task.Description, task.DueDate });
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var task in SeedData_InitialTasks.InitialTasks)
            {
                migrationBuilder.DeleteData(
                    table: "TodoItems",
                    keyColumn: "Id",
                    keyValue: task.Id);
            }
        }
    }
}
