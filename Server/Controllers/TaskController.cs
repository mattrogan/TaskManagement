using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
        var tasks = taskRepository;
        return Ok(tasks);
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
