using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using System.Net;
using TaskManagement.Server.Constants;
using TaskManagement.Server.Controllers;
using TaskManagement.Server.Data;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace TaskManagement.Tests.ServerTests.ControllerTests
{
    [TestClass]
    public class TodoItemControllerTests
    {
        private Mock<ITaskContext> mockTaskContext;
        private Mock<ILogger<TodoItemController>> mockLogger;
        private Mock<IMapper> mockMapper;

        [TestInitialize]
        public void Initialize()
        {
            mockTaskContext = new(MockBehavior.Strict);
            mockLogger = new(MockBehavior.Strict);
            mockMapper = new(MockBehavior.Strict);
        }

        [TestCleanup]
        public void Cleanup()
        {
            mockTaskContext.Verify();
            mockLogger.Verify();
            mockMapper.Verify();
        }

        #region Get
        [TestMethod]
        public async Task TodoItemController_GetTasksAsync_ReturnsCollectionOfTasks()
        {
            var tasks = Enumerable.Range(0, 5)
                .Select(x => new TodoItem() { Id = x })
                .ToList();

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(tasks)
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.GetTasksAsync();
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var returnTasks = (result as OkObjectResult).Value as IQueryable<TodoItem>;
            Assert.IsNotNull(returnTasks);
            Assert.AreEqual(5, returnTasks.Count());
        }

        [TestMethod]
        public async Task TodoItemController_GetTaskAsync_ReturnsNotFoundWhenTaskDoesntExist()
        {
            var id = new Random().Next();

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>())
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.GetTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual(id, (result as NotFoundObjectResult).Value);
        }

        [TestMethod]
        public async Task TodoItemController_GetTaskAsync_ReturnsRequestedTask()
        {
            var id = new Random().Next();
            var dateNow = DateTime.Now;

            var task = new TodoItem
            {
                Id = id,
                Title = "Foo",
                Description = "Bar",
                DueDate = dateNow,
                IsCompleted = false,
            };

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>() { task })
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.GetTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var returnedTask = (result as OkObjectResult).Value as TodoItem;
            Assert.IsNotNull(returnedTask);

            Assert.AreEqual(task.Id, returnedTask.Id);
            Assert.AreEqual(task.Title, returnedTask.Title);
            Assert.AreEqual(task.Description, returnedTask.Description);
            Assert.AreEqual(task.DueDate, returnedTask.DueDate);
            Assert.AreEqual(task.IsCompleted, returnedTask.IsCompleted);
        }

        [TestMethod]
        public async Task TodoItemController_GetCompletedTasksAsync_ReturnsCompletedTasks()
        {
            var tasks = new List<TodoItem>
            {
                new TodoItem{ IsCompleted = true },
                new TodoItem{ IsCompleted = false },
                new TodoItem{ IsCompleted = true },
                new TodoItem{ IsCompleted = true },
                new TodoItem{ IsCompleted = false },
            };

            mockTaskContext
               .SetupGet(x => x.TodoItems)
               .ReturnsDbSet(tasks)
               .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.GetCompletedTasksAsync();
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var returnTasks = (result as OkObjectResult).Value as IQueryable<TodoItem>;
            Assert.IsNotNull(returnTasks);
            Assert.AreEqual(3, returnTasks.Count());
        }
        #endregion

        #region Post
        // BadRequest when model null
        [TestMethod]
        public async Task TodoItemController_PostTaskAsync_ReturnsBadRequestWhenModelNull()
        {
            var ctrl = SetupController();

            var result = await ctrl.PostTaskAsync(null);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // BadRequest when model invalid
        [TestMethod]
        public async Task TodoItemController_PostTaskAsync_ReturnsBadRequestWhenModelInvalid()
        {
            var ctrl = SetupController();
            ctrl.ModelState.AddModelError("*", "error");

            var result = await ctrl.PostTaskAsync(new PostTodoItem());
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var errors = (result as BadRequestObjectResult).Value as SerializableError;
            Assert.AreEqual("*", errors.First().Key);
            Assert.AreEqual("error", (errors.First().Value as string[])[0]);
        }

        // InternalServerError when save fails
        [TestMethod]
        public async Task TodoItemController_PostTaskAsync_ReturnsInternalServerErrorWhenSaveFails()
        {
            var model = new PostTodoItem
            {
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now
            };

            mockMapper
                .Setup(x => x.Map(It.IsAny<PostTodoItem>(), It.IsAny<TodoItem>()))
                .Returns((PostTodoItem i1, TodoItem i2) => i2)
                .Verifiable();

            mockTaskContext
                .Setup(x => x.AddAsync(It.IsAny<TodoItem>()))
                .Throws(new Exception())
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.PostTaskAsync(model);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));

            var statusResult = (result as ObjectResult).StatusCode;
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, statusResult);
        }

        [TestMethod]
        public async Task TodoItemController_PostItemAsync_ReturnsCreatedWhenAllOk()
        {
            var model = new PostTodoItem
            {
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now
            };

            mockMapper
                .Setup(x => x.Map(It.IsAny<PostTodoItem>(), It.IsAny<TodoItem>()))
                .Returns((PostTodoItem i1, TodoItem i2) => new TodoItem
                {
                    Title = i1.Title,
                    Description = i1.Description,
                    DueDate = i1.DueDate,
                    IsCompleted = false
                })
                .Verifiable();

            mockTaskContext
                .Setup(x => x.AddAsync(It.IsAny<TodoItem>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockTaskContext
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.PostTaskAsync(model);
            Assert.IsInstanceOfType(result, typeof(CreatedResult));
        }
        #endregion

        #region Put
        [TestMethod]
        public async Task TodoItemController_PutTaskAsync_ReturnsBadRequestWhenModelNull()
        {
            var ctrl = SetupController();

            var result = await ctrl.PutTaskAsync(new Random().Next(), null);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task TodoItemController_PutTaskAsync_ReturnsBadRequestWhenModelInvalid()
        {
            var ctrl = SetupController();
            ctrl.ModelState.AddModelError("*", "error");

            var result = await ctrl.PutTaskAsync(new Random().Next(), new PostTodoItem());
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var errors = (result as BadRequestObjectResult).Value as SerializableError;
            Assert.AreEqual("*", errors.First().Key);
            Assert.AreEqual("error", (errors.First().Value as string[])[0]);
        }

        [TestMethod]
        public async Task TodoItemController_PutTaskAsync_ReturnsNotFoundWhenTaskDoesntExist()
        {
            var id = new Random().Next();

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>())
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.PutTaskAsync(id, new PostTodoItem());
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual(id, (result as NotFoundObjectResult).Value);
        }

        [TestMethod]
        public async Task TodoItemController_PutTaskAsync_ReturnsInternalServerErrorWhenUpdateFails()
        {
            var id = new Random().Next();
            var model = new PostTodoItem()
            {
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now.AddDays(5)
            };

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>() { new TodoItem() { Id = id } })
                .Verifiable();

            mockMapper
                .Setup(x => x.Map(It.IsAny<PostTodoItem>(), It.IsAny<TodoItem>()))
                .Returns((PostTodoItem i1, TodoItem i2) => i2)
                .Verifiable();

            mockTaskContext
                .Setup(x => x.SaveChangesAsync())
                .Throws(new Exception())
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.PutTaskAsync(id, model);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, (result as ObjectResult).StatusCode);
        }

        [TestMethod]
        public async Task TodoItemController_PutTaskAsync_ReturnsOkAndUpdatesItem()
        {
            var id = new Random().Next();
            var model = new PostTodoItem()
            {
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now.AddDays(5)
            };

            var task = new TodoItem()
            {
                Id = id,
                Title = "Old Foo",
                Description = "Old Bar",
                DueDate = DateTime.Now,
                IsCompleted = false
            };

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>() { task })
                .Verifiable();

            mockMapper
                .Setup(x => x.Map(It.IsAny<PostTodoItem>(), It.IsAny<TodoItem>()))
                .Returns((PostTodoItem i1, TodoItem i2) =>
                {
                    i2.Title = i1.Title;
                    i2.Description = i1.Description;
                    i2.DueDate = i1.DueDate;
                    return i2;
                })
                .Verifiable();

            mockTaskContext
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.PutTaskAsync(id, model);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var updatedTask = (result as OkObjectResult).Value as TodoItem;
            Assert.IsNotNull(updatedTask);
            Assert.AreEqual(model.Title, updatedTask.Title);
            Assert.AreEqual(model.Description, updatedTask.Description);
            Assert.AreEqual(model.DueDate, updatedTask.DueDate);
        }

        [TestMethod]
        public async Task TodoItemController_CompleteTaskAsync_ReturnsNotFoundWhenTaskDoesntExist()
        {
            var id = new Random().Next();

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>())
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.CompleteTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual(id, (result as NotFoundObjectResult).Value);
        }

        [TestMethod]
        public async Task TodoItemController_CompleteTaskAsync_ReturnsBadRequestIfTaskAlreadyCompleted()
        {
            var id = new Random().Next();
            var task = new TodoItem
            {
                Id = id,
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now,
                IsCompleted = true
            };

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>() { task })
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.CompleteTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual(TodoItemConstants.ERROR_TASKALREADYCOMPLETED, (result as BadRequestObjectResult).Value);
        }

        [TestMethod]
        public async Task TodoItemController_CompleteTaskAsync_ReturnsOkWhenTaskCompleted()
        {
            var id = new Random().Next();
            var task = new TodoItem
            {
                Id = id,
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now,
                IsCompleted = false
            };

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>() { task })
                .Verifiable();

            mockTaskContext
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.CompleteTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var completedTask = (result as OkObjectResult).Value as TodoItem;
            Assert.IsNotNull(completedTask);
            Assert.IsTrue(completedTask.IsCompleted);
        }

        #endregion

        #region Delete
        [TestMethod]
        public async Task TodoItemController_DeleteTaskAsync_ReturnsNotFoundWhenTaskDoesntExist()
        {
            var id = new Random().Next();

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>())
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.DeleteTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            Assert.AreEqual(id, (result as NotFoundObjectResult).Value);
        }

        [TestMethod]
        public async Task TodoItemController_DeleteTaskAsync_ReturnsInternalServerErrorWhenDeleteFails()
        {
            var id = new Random().Next();
            var task = new TodoItem
            {
                Id = id,
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now,
                IsCompleted = true
            };

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>() { task })
                .Verifiable();

            mockTaskContext
                .Setup(x => x.Remove(task))
                .Throws(new Exception())
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.DeleteTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, (result as ObjectResult).StatusCode);
        }

        [TestMethod]
        public async Task TodoItemController_DeleteTaskAsync_ReturnsNoContentWhenTaskDeleted()
        {
            var id = new Random().Next();
            var task = new TodoItem
            {
                Id = id,
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now,
                IsCompleted = true
            };

            mockTaskContext
                .SetupGet(x => x.TodoItems)
                .ReturnsDbSet(new List<TodoItem>() { task })
                .Verifiable();

            mockTaskContext
                .Setup(x => x.Remove(task))
                .Verifiable();

            mockTaskContext
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var ctrl = SetupController();

            var result = await ctrl.DeleteTaskAsync(id);
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
        #endregion

        internal TodoItemController SetupController()
            => new TodoItemController(mockLogger.Object, mockMapper.Object, mockTaskContext.Object);

    }
}
