using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Net;
using TaskManagement.Server.Data;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoItemController : ControllerBase
    {
        private readonly ILogger<TodoItemController> logger;
        private readonly IMapper mapper;
        private readonly ITaskContext ctx;

        public TodoItemController(ILogger<TodoItemController> logger, IMapper mapper, ITaskContext ctx)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.ctx = ctx;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasksAsync()
        {
            var todoItems = await ctx.TodoItems.ToListAsync();
            return Ok(todoItems.AsQueryable());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskAsync(int id)
        {
            var todoItem = await ctx.TodoItems.SingleOrDefaultAsync(t => t.Id == id);
            if (todoItem == null)
            {
                return NotFound(id);
            }

            return Ok(todoItem);
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedTasksAsync()
        {
            var completedTasks = await ctx.TodoItems.Where(t => t.IsCompleted).ToListAsync();
            return Ok(completedTasks.AsQueryable());
        }

        [HttpPost]
        public async Task<IActionResult> PostTaskAsync(PostTodoItem model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = mapper.Map(model, new TodoItem());

            await ctx.AddAsync(task);
            await ctx.SaveChangesAsync();

            return Created(nameof(PostTaskAsync), task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskAsync(int id, PostTodoItem model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = await ctx.TodoItems.FindAsync(id);
            if (task == null)
            {
                return NotFound(id);
            }

            try
            {
                mapper.Map(model, task);
                await ctx.SaveChangesAsync();
                return Ok(task);
            }
            catch (DbException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut("completeTask({id})")]
        public async Task<IActionResult> CompleteTaskAsync(int id)
        {
            var task = await ctx.TodoItems.FindAsync(id);
            if (task == null)
            {
                return NotFound(id);
            }

            if (task.IsCompleted)
            {
                return BadRequest("Cannot complete a task that is already completed");
            }

            task.IsCompleted = true;
            await ctx.SaveChangesAsync();

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskAsync(int id)
        {
            var task = await ctx.TodoItems.FindAsync(id);
            if (task == null)
            {
                return NotFound(id);
            }

            try
            {
                ctx.Remove(task);
                await ctx.SaveChangesAsync();
            }
            catch (DbException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }

            return NoContent();
        }
    }
}
