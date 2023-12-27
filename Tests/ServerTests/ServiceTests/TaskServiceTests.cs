using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Moq;
using Moq.EntityFrameworkCore;
using TaskManagement.Server.Data;
using TaskManagement.Server.Services;
using TaskManagement.Shared.Models;

namespace TaskManagement.Tests.ServerTests.ServiceTests
{
    [TestClass]
    public class TaskServiceTests
    {
        private Mock<ITaskContext> mockContext;

        [TestInitialize]
        public void Initialise()
        {
            mockContext = new(MockBehavior.Strict);
        }

        [TestCleanup]
        public void Cleanup()
        {
            mockContext.Verify();
        }

        [TestMethod]
        public async Task TaskService_GetTasksAsync_ReturnsTasksInContext()
        {
            var tasks = new List<TodoItem>
            {
                new TodoItem(),
                new TodoItem(),
                new TodoItem(),
                new TodoItem(),
                new TodoItem()
            };

            mockContext
                .SetupGet(ctx => ctx.TodoItems)
                .ReturnsDbSet(tasks)
                .Verifiable();

            var svc = GetService();

            var result = await svc.GetTasksAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count());
        }

        [TestMethod]
        public async Task TaskService_GetTaskAsync_ReturnsNullIfDoesntExist()
        {
            var tasks = new List<TodoItem>
            {
                new TodoItem(),
                new TodoItem(),
                new TodoItem(),
                new TodoItem(),
                new TodoItem()
            };

            mockContext
                .SetupGet(ctx => ctx.TodoItems)
                .ReturnsDbSet(tasks)
                .Verifiable();

            var svc = GetService();

            var result = await svc.GetTaskAsync(new Random().Next());
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TaskService_GetTaskAsync_ReturnsRequestedTask()
        {
            var id = new Random().Next();
            var tasks = new List<TodoItem> { new TodoItem() { Id = id } };

            mockContext
                .SetupGet(ctx => ctx.TodoItems)
                .ReturnsDbSet(tasks)
                .Verifiable();

            var svc = GetService();

            var result = await svc.GetTaskAsync(id);
            Assert.IsNotNull(result);
            Assert.AreEqual(result, tasks.First());
        }

        [TestMethod]
        public async Task TaskService_AddTaskAsync_ReturnsFalseWhenAddFails()
        {
            var id = new Random().Next();
            var task = new TodoItem { Id = id };

            mockContext
                .Setup(ctx => ctx.AddAsync(task))
                .Throws(new Exception())
                .Verifiable();

            var svc = GetService();

            var result = await svc.AddTaskAsync(task);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TaskService_AddTaskAsync_ReturnsTrueWhenTaskAdded()
        {
            var id = new Random().Next();
            var task = new TodoItem { Id = id };

            mockContext
                .Setup(ctx => ctx.AddAsync(task))
                .Returns(Task.FromResult(true))
                .Verifiable();

            mockContext
                .Setup(ctx => ctx.SaveChangesAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var svc = GetService();

            var result = await svc.AddTaskAsync(task);
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TaskService_UpdateTaskAsync_ReturnsFalseWhenSaveFails()
        {
            var id = new Random().Next();
            var task = new TodoItem { Id = id };

            mockContext
                .Setup(ctx => ctx.TodoItems.Update(It.IsAny<TodoItem>()))
                .Returns(It.IsAny<EntityEntry<TodoItem>>())
                .Verifiable();

            mockContext
                .Setup(ctx => ctx.SaveChangesAsync())
                .Throws(new DbUpdateException())
                .Verifiable();

            var svc = GetService();

            var result = await svc.UpdateTaskAsync(task);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TaskService_UpdateTaskAsync_ReturnsTrueWhenTaskUpdated()
        {
            var id = new Random().Next();
            var task = new TodoItem { Id = id };

            mockContext
                .Setup(ctx => ctx.TodoItems.Update(It.IsAny<TodoItem>()))
                .Returns(It.IsAny<EntityEntry<TodoItem>>())
                .Verifiable();

            mockContext
                .Setup(ctx => ctx.SaveChangesAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var svc = GetService();

            var result = await svc.UpdateTaskAsync(task);
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TaskService_DeleteTaskAsync_ReturnsFalseWhenDeletionFails()
        {
            var id = new Random().Next();
            var task = new TodoItem { Id = id };

            mockContext
                .Setup(ctx => ctx.Remove(task))
                .Verifiable();

            mockContext
                .Setup(ctx => ctx.SaveChangesAsync())
                .Throws(new DbUpdateException())
                .Verifiable();

            var svc = GetService();

            var result = await svc.DeleteTaskAsync(task);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TaskService_DeleteTaskAsync_ReturnsTrueWhenTaskDeleted()
        {
            var id = new Random().Next();
            var task = new TodoItem { Id = id };

            mockContext
                .Setup(ctx => ctx.Remove(task))
                .Verifiable();

            mockContext
                .Setup(ctx => ctx.SaveChangesAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var svc = GetService();

            var result = await svc.DeleteTaskAsync(task);
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        private ITaskService GetService()
        {
            return new TaskService(mockContext.Object);
        }
    }
}
