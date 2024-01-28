using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.UnitOfWork;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TaskController : ControllerBase
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IRepository<TodoItem> taskRepository;
    private readonly IMapper mapper;
    private readonly ILogger<TaskController> logger;

    public TaskController(IUnitOfWork unitOfWork, ILogger<TaskController> logger, IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.taskRepository = unitOfWork.GetRepository<TodoItem>();
        this.mapper = mapper;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasksAsync()
    {
        var tasks = await taskRepository.FindAsync(x => true);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> PostTaskAsync(PostTodoItem model)
    {
        if (model == null || !ModelState.IsValid)
        {
            logger.LogInformation("The model was not valid");
            return ValidationProblem(ModelState);
        }

        var task = mapper.Map<TodoItem>(model);
        await taskRepository.AddAsync(task);

        return Created(nameof(PostTaskAsync), task);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskAsync(int id)
    {
        var task = await taskRepository.SingleAsync(id);
        if (task == null)
        {
            this.logger.LogInformation("Could not find task with id {TaskId}", id);
            return NotFound(id);
        }

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTaskAsync(int id)
    {
        // Find the task
        var task = await taskRepository.SingleAsync(id);
        if (task == null)
        {
            return NotFound(id);
        }

        if (!await taskRepository.DeleteAsync(task))
        {
            this.logger.LogError("Unable to delete task with id {TaskId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        return NoContent();
    }
}
