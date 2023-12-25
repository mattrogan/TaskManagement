using Microsoft.AspNetCore.Mvc;
using TaskManagement.Shared.Models;
using System.Linq;

namespace TaskManagement.Server.Controllers
{
    [ApiController]
    [Route("[controller]/")]
    public class TodoItemController : ControllerBase
    {
        private readonly ILogger<TodoItemController> logger;

        private static readonly IEnumerable<TodoItem> Tasks = new[] {
            new TodoItem("Foo", "Bar", DateTime.Now),
            new TodoItem("Lorem Ipsum", "Dolor Sit", DateTime.Now),
        };

        public TodoItemController(ILogger<TodoItemController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasksAsync()
            => Ok(Tasks.ToList());
    }
}
