using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Server.Controllers;
using TaskManagement.Server.Data;
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

        [TestMethod]
        public async Task TodoItemController_GetTasksAsync_ShouldReturnCollectionOfTasks()
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
        public async Task TodoItemController_GetTaskAsync_ShouldReturnNotFoundWhenTaskDoesntExist()
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
        public async Task TodoItemController_GetTaskAsync_ShouldReturnRequestedTask()
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

        internal TodoItemController SetupController()
            => new TodoItemController(mockLogger.Object, mockMapper.Object, mockTaskContext.Object);
           
    }
}
