using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.UnitOfWork;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TaskController : BaseController<TodoItem>
{
    private readonly IRepository<TodoItem> taskRepository;

    public TaskController(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
        this.taskRepository = unitOfWork.GetRepository<TodoItem>();
    }

    #region Get Tasks
    [HttpGet]
    public async Task<IActionResult> GetTasksAsync()
    {
        var tasks = await taskRepository.FindAsync(x => true);
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

    #region Delete Tasks
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
    #endregion
}
