using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Controllers;
using Server.UnitOfWork;
using TaskManagement.Shared.Models;

namespace Tests.ServerTests.ControllerTests;

[TestClass]
public class TaskControllerTests
{
    private Mock<IUnitOfWork> mockUnitOfWork;
    private Mock<IRepository<TodoItem>> mockTaskRepository;
    private Mock<ILogger<TaskController>> mockLogger;
    private Mock<IMapper> mockMapper;


    [TestInitialize]
    public void Initialize()
    {
        mockUnitOfWork = new(MockBehavior.Strict);
        mockTaskRepository = new(MockBehavior.Strict);
        mockLogger = new(MockBehavior.Strict);
        mockMapper = new(MockBehavior.Strict);
    }   

    [TestCleanup]
    public void Cleanup()
    {
        mockUnitOfWork.Verify();
        mockTaskRepository.Verify();
        mockLogger.Verify();
        mockMapper.Verify();
    } 

    [TestMethod]
    public async Task GetTasksAsyncShouldReturnCollectionOfTasks()
    {
        var tasks = new List<TodoItem>
        {
            new TodoItem {
                Id = 1,
                Title = "Foo",
                Description = "Testing item 1",
                DueDate = DateTime.Now.AddDays(3),
                IsCompleted = false
            },

            new TodoItem {
                Id = 2,
                Title = "Bar",
                Description = "Testing item 2",
                DueDate = DateTime.Now.AddDays(-1),
                IsCompleted = true
            },

            new TodoItem {
                Id = 3,
                Title = "Baz",
                Description = "Testing item 3",
                DueDate = DateTime.Now.AddDays(-1),
                IsCompleted = false
            }
        };

        mockUnitOfWork
            .Setup(x => x.GetRepository<TodoItem>())
            .Returns(mockTaskRepository.Object)
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TodoItem, bool>>>()))
            .Returns(Task.FromResult(tasks.AsEnumerable()))
            .Verifiable();

        var subject = GetTaskController();

        var result = await subject.GetTasksAsync();
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var response = okResult.Value as IEnumerable<TodoItem>;
        Assert.IsNotNull(response);
        Assert.AreEqual(3, response.Count());
        Assert.IsTrue(Enumerable.Range(1, 3).All(x => response.Any(t => t.Id == x)));
    }

    [TestMethod]
    public async Task GetTaskAsyncShouldReturnNotFoundWhenTaskDoesntExist()
    {
        var id = new Random().Next();
        
        mockUnitOfWork
            .Setup(x => x.GetRepository<TodoItem>())
            .Returns(mockTaskRepository.Object)
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult<TodoItem>(null))
            .Verifiable();

        var subject = GetTaskController();
        var result = await subject.GetTaskAsync(id);
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(id, notFoundResult.Value);
    }

    [TestMethod]
    public async Task GetTaskAsyncShouldReturnRequestedTask()
    {
        var id = new Random().Next();
        var task = new TodoItem { Id = id };

        mockUnitOfWork
            .Setup(x => x.GetRepository<TodoItem>())
            .Returns(mockTaskRepository.Object)
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult(task))
            .Verifiable();

        var subject = GetTaskController();
        var result = await subject.GetTaskAsync(id);
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(task, okResult.Value);
    }


    internal TaskController GetTaskController()
        => new TaskController(mockUnitOfWork.Object, mockLogger.Object, mockMapper.Object);
}