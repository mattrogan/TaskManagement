using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.UnitOfWork;
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
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskAsync(int id)
    {
        var task = await taskRepository.SingleAsync(id);
        if (task == null)
        {
            return NotFound(id);
        }
        
        return Ok(task);
    }
}