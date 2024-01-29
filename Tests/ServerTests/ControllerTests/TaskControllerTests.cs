using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Controllers;
using Server.UnitOfWork;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace Tests.ServerTests.ControllerTests;

[TestClass]
public class TaskControllerTests
{

    internal Mock<IUnitOfWork> mockUnitOfWork;
    internal Mock<ILogger<TaskController>> mockLogger;
    internal Mock<IMapper> mockMapper;
    internal Mock<IRepository<TodoItem>> mockTaskRepository;

    [TestInitialize]
    public void Initialize()
    {
        mockUnitOfWork = new(MockBehavior.Strict);
        mockLogger = new(MockBehavior.Strict);
        mockMapper = new(MockBehavior.Strict);
        mockTaskRepository = new(MockBehavior.Strict);

        mockUnitOfWork
            .Setup(x => x.GetRepository<TodoItem>())
            .Returns(mockTaskRepository.Object)
            .Verifiable();

        mockLogger
            .Setup(x => x.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(), 
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    [TestCleanup]
    public void Cleanup()
    {
        mockUnitOfWork.Verify();
        mockLogger.Verify();
        mockMapper.Verify();
        mockTaskRepository.Verify();
    }

    #region Get
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
    #endregion

    #region Post
    [TestMethod]
    public async Task PostTaskAsyncShouldReturnValidationProblemWhenModelIsNull()
    {
        var subject = GetTaskController();
        var result = await subject.PostTaskAsync(null);

        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);

        var problems = objectResult.Value as ValidationProblemDetails;
        Assert.IsNotNull(problems);
        Assert.AreEqual(1, problems.Errors.Count());

        var error = problems.Errors.First();
        Assert.AreEqual("*", error.Key);
        Assert.AreEqual(ControllerConstants.POSTMODEL_NULL_ERROR, error.Value.First());
    }

    [TestMethod]
    public async Task PostTaskAsyncShouldReturnValidationProblemWhenModelInvalid()
    {
        var subject = GetTaskController();
        subject.ModelState.AddModelError("*", "error");
        var result = await subject.PostTaskAsync(new PostTodoItem());

        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);

        var problems = objectResult.Value as ValidationProblemDetails;
        Assert.IsNotNull(problems);
        Assert.AreEqual(1, problems.Errors.Count());

        var error = problems.Errors.First();
        Assert.AreEqual("*", error.Key);
        Assert.AreEqual("error", error.Value.First());
    }

    [TestMethod]
    public async Task PostTaskAsyncShouldReturnCreatedWithNewItem()
    {
        mockMapper
            .Setup(x => x.Map(It.IsAny<PostTodoItem>(), It.IsAny<TodoItem>()))
            .Returns((PostTodoItem vm, TodoItem i) => i)
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.AddAsync(It.IsAny<TodoItem>()))
            .Returns(Task.FromResult(new TodoItem()))
            .Verifiable();

        var model = new PostTodoItem
        {
            Title = "Foo",
            Description = "Bar",
            DueDate = DateTime.Now.AddDays(3)
        };

        var subject = GetTaskController();
        var result = await subject.PostTaskAsync(model);

        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual((int)HttpStatusCode.Created, objectResult.StatusCode);
    }
    #endregion

    #region Put
    [TestMethod]
    public async Task PutTaskAsyncShouldReturnValidationProblemWhenModelNull()
    {
        var subject = GetTaskController();
        var result = await subject.PutTaskAsync(new Random().Next(), null);
        Assert.IsInstanceOfType(result, typeof(ObjectResult));

        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);

        var problems = objectResult.Value as ValidationProblemDetails;
        Assert.IsNotNull(problems);
        Assert.AreEqual(1, problems.Errors.Count());

        var error = problems.Errors.First();
        Assert.AreEqual("*", error.Key);
        Assert.AreEqual(ControllerConstants.POSTMODEL_NULL_ERROR, error.Value.First());
    }

    [TestMethod]
    public async Task PutTaskAsyncShouldReturnValidationProblemWhenModelInvalid()
    {
        var subject = GetTaskController();
        subject.ModelState.AddModelError("*", "error");

        var result = await subject.PutTaskAsync(new Random().Next(), new PostTodoItem());
        Assert.IsInstanceOfType(result, typeof(ObjectResult));

        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);

        var problems = objectResult.Value as ValidationProblemDetails;
        Assert.IsNotNull(problems);
        Assert.AreEqual(1, problems.Errors.Count());

        var error = problems.Errors.First();
        Assert.AreEqual("*", error.Key);
        Assert.AreEqual("error", error.Value.First());
    }

    [TestMethod]
    public async Task PutTaskAsyncShouldReturnNotFoundWhenTaskDoesntExist()
    {
        var id = new Random().Next();
        
        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult<TodoItem>(null))
            .Verifiable();

        var subject = GetTaskController();
        var result = await subject.PutTaskAsync(id, new PostTodoItem());
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(id, notFoundResult.Value);
    }
    
    [TestMethod]
    public async Task PutTaskAsyncShouldReturnInternalServerErrorWhenUpdateFails()
    {
        var id = new Random().Next();
        
        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult(new TodoItem(){ Id = id }))
            .Verifiable();

        mockMapper
            .Setup(x => x.Map(It.IsAny<PostTodoItem>(), It.IsAny<TodoItem>()))
            .Returns((PostTodoItem vm, TodoItem i) => i)
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.UpdateAsync(It.IsAny<TodoItem>()))
            .Returns(Task.FromResult(false))
            .Verifiable();

        var subject = GetTaskController();
        var result = await subject.PutTaskAsync(id, new PostTodoItem());
        Assert.IsInstanceOfType(result, typeof(StatusCodeResult));

        var statusResult = result as StatusCodeResult;
        Assert.IsNotNull(statusResult);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, statusResult.StatusCode);
    }
    
    [TestMethod]
    public async Task PutTaskAsyncShouldReturnOkAndUpdateTask()
    {
        var id = new Random().Next();
        var task = new TodoItem
        {
            Id = id,
            Title = "Old",
            Description = "Older",
            DueDate = DateTime.Now.AddDays(-3),
            IsCompleted = false
        };

        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult(task))
            .Verifiable();

        mockMapper
            .Setup(x => x.Map(It.IsAny<PostTodoItem>(), It.IsAny<TodoItem>()))
            .Returns((PostTodoItem vm, TodoItem i) =>
            {
                i.Title = vm.Title;
                i.Description = vm.Description;
                i.DueDate = vm.DueDate;

                return i;
            })
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.UpdateAsync(It.IsAny<TodoItem>()))
            .Returns(Task.FromResult(true))
            .Verifiable();

        var model = new PostTodoItem
        {
            Title = "New",
            Description = "Newer",
            DueDate = DateTime.Now.AddDays(7)
        };

        var subject = GetTaskController();
        var result = await subject.PutTaskAsync(id, model);
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var returnTask = okResult.Value as TodoItem;
        Assert.IsNotNull(returnTask);
        Assert.AreEqual(model.Title, returnTask.Title);
        Assert.AreEqual(model.Description, returnTask.Description);
        Assert.AreEqual(model.DueDate, returnTask.DueDate);
    }
    #endregion

    #region Delete
    [TestMethod]
    public async Task DeleteTaskAsyncShouldReturnNotFoundWhenTaskDoesntExist()
    {
        var id = new Random().Next();

        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult<TodoItem>(null))
            .Verifiable();

        var subject = GetTaskController();
        var result = await subject.DeleteTaskAsync(id);
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));

        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(id, notFoundResult.Value);
    }

    [TestMethod]
    public async Task DeleteTaskAsyncShouldReturnInternalServerErrorWhenDeleteFails()
    {
        var id = new Random().Next();
        var task = new TodoItem { Id = id };

        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult(task))
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.DeleteAsync(task))
            .Returns(Task.FromResult(false))
            .Verifiable();

        var subject = GetTaskController();
        var result = await subject.DeleteTaskAsync(id);
        Assert.IsInstanceOfType(result, typeof(StatusCodeResult));

        var statusResult = result as StatusCodeResult;
        Assert.IsNotNull(statusResult);
        Assert.AreEqual((int)HttpStatusCode.InternalServerError, statusResult.StatusCode);
    }

    [TestMethod]
    public async Task DeleteTaskAsyncShouldReturnNoContentAndDeleteTask()
    {
        var id = new Random().Next();
        var task = new TodoItem { Id = id };

        mockTaskRepository
            .Setup(x => x.SingleAsync(id))
            .Returns(Task.FromResult(task))
            .Verifiable();

        mockTaskRepository
            .Setup(x => x.DeleteAsync(task))
            .Returns(Task.FromResult(true))
            .Verifiable();

        var subject = GetTaskController();
        var result = await subject.DeleteTaskAsync(id);
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }
    #endregion

    internal TaskController GetTaskController()
    {
        return new TaskController(mockUnitOfWork.Object, mockLogger.Object, mockMapper.Object);
    }
}