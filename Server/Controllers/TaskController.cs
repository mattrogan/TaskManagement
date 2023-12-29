using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using TaskManagement.Server.Services;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IServiceProvider<TodoItem> serviceProvider;
        private readonly ILogger<TaskController> logger;
        private readonly IMapper mapper;

        public TaskController(IServiceProvider<TodoItem> serviceProvider, ILogger<TaskController> logger, IMapper mapper)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasksAsync()
        {
            var tasks = await this.serviceProvider.GetAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskAsync(int id)
        {
            var task = await this.serviceProvider.GetSingleAsync(id);
            if (task == null)
            {
                this.logger.LogInformation("No task found with id {TaskId}", id);
                return NotFound(id);
            }

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> PostTaskAsync(PostTodoItem model)
        {
            if (model == null || !this.ModelState.IsValid)
            {
                this.logger.LogInformation("Model was null or invalid");
                return BadRequest(ModelState);
            }

            try
            {
                var task = this.mapper.Map(model, new TodoItem());
                await this.serviceProvider.AddAsync(task);
                return Created(nameof(this.PostTaskAsync), task);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
