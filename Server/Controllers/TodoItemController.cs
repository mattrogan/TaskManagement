using Microsoft.AspNetCore.Mvc;
using TaskManagement.Shared.Models;
using System.Linq;
using TaskManagement.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Server.Controllers
{
    [ApiController]
    [Route("[controller]/")]
    public class TodoItemController : ControllerBase
    {
        private readonly ILogger<TodoItemController> logger;
        private readonly TaskContext ctx;

        public TodoItemController(ILogger<TodoItemController> logger, TaskContext ctx)
        {
            this.logger = logger;
            this.ctx = ctx;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasksAsync()
        {
            var todoItems = await ctx.TodoItems.ToListAsync();
            return Ok(todoItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskAsync(int id)
        {
            var todoItem = await ctx.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound(id);
            }

            return Ok(todoItem);
        }
    }
}
