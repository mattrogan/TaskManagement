using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Server.UnitOfWork;
using Server.ViewModels;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TaskController : BaseController
{
    private readonly IRepository<TodoItem> taskRepository;

    public TaskController(IUnitOfWork unitOfWork, ILogger<TaskController> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
        this.taskRepository = unitOfWork.GetRepository<TodoItem>();
    }

    #region Get Tasks
    [HttpGet]
    public async Task<IActionResult> GetTasksAsync()
    {
        var tasks = await taskRepository.QueryAsync(t => true, t => new GetTodoItem{
            Title = t.Title,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted
        });
        return Ok(tasks.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskAsync(int id)
    {
        var results = await taskRepository.QueryAsync(t => t.Id == id, t => new GetTodoItem{
            Title = t.Title,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted
        });

        var task = results.FirstOrDefault();

        return task == null
            ? NotFound(id)
            : Ok(task);
    }

    [HttpGet("completedTasks")]
    public async Task<IActionResult> GetCompletedTasks()
    {
        var completedTasks = await taskRepository.QueryAsync(t => t.IsCompleted, t => new CompletedTask
        {
            Title = t.Title,
            Description = t.Description,
            DueDate = t.DueDate
        });
        return Ok(completedTasks.ToListAsync());
    }
    #endregion

    #region Post Tasks
    [HttpPost]
    public async Task<IActionResult> PostTaskAsync(PostTodoItem model)
    {
        if (model == null || !ModelState.IsValid)
        {
            logger.LogInformation("The model was not valid");
            
            if (model == null)
            {
                ModelState.AddModelError("*", ControllerConstants.POSTMODEL_NULL_ERROR);
            }

            return ValidationProblem(ModelState);
        }

        var task = mapper.Map(model, new TodoItem());
        await taskRepository.AddAsync(task);

        return Created(nameof(PostTaskAsync), task);
    }
    
    [HttpPost("completeTasks")]
    public async Task<IActionResult> CompleteTasks([FromBody] IEnumerable<CompleteTodoItem> model, CancellationToken ct = default)
    {
        if (model == null || !ModelState.IsValid)
        {
            return BadRequest();
        }

        // Deserialize and assert that all items correspond
        var tasks = await taskRepository.QueryAsync(
            t => model.Select(m => m.Id).Contains(t.Id),
            t => t
        ).ConfigureAwait(false);

        // Verify that all tasks posted exist in the database
        var nonExistentTasks = tasks.Where(t => !model.Select(m => m.Id).Any(m => m == t.Id));
        if (nonExistentTasks.Any())
            return NotFound(nonExistentTasks.Select(t => t.Id));

        // Set all task complete values
        foreach (var task in tasks)
            task.IsCompleted = model.Single(t => t.Id == task.Id).Complete;

        if (!await taskRepository.UpdateAsync(tasks))
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        return Ok(tasks);
    }

    #endregion

    #region Put Tasks
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTaskAsync(int id, PostTodoItem model)
    {
        if (model == null || !ModelState.IsValid)
        {
            logger.LogInformation("The model was not valid");

            if (model == null)
            {
                ModelState.AddModelError("*", ControllerConstants.POSTMODEL_NULL_ERROR);
            }

            return ValidationProblem(ModelState);
        }

        // Find the existing task
        var task = await taskRepository.SingleAsync(id);
        if (task == null)
        {
            logger.LogInformation("Could not find task with id {TaskId}", id);
            return NotFound(id);
        }

        mapper.Map(model, task);
        
        if (!await taskRepository.UpdateAsync(task))
        {
            logger.LogWarning("Failed to update task with id {TaskId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        return Ok(task);
    }
    #endregion

    #region Delete Tasks
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTaskAsync(int id)
    {
        // Find the task
        var task = await taskRepository.SingleAsync(id);
        if (task == null)
        {
            logger.LogInformation("Could not find task with id {TaskId}", id);
            return NotFound(id);
        }

        if (!await taskRepository.DeleteAsync(task))
        {
            this.logger.LogError("Unable to delete task with id {TaskId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        return NoContent();
    }
    #endregion
}
