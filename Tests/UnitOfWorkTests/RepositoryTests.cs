using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.UnitOfWork;
using TaskManagement.Server.Data;
using TaskManagement.Server.MappingProfiles;
using TaskManagement.Shared.Models;

namespace Tests.UnitOfWorkTests
{
    [TestClass]
    public class RepositoryTests
    {
        private TaskContext context;
        private IRepository<TodoItem> repository;

        [TestInitialize]
        public void Initialise()
        {
            // Configure test context
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var opts = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase("TestDatabase")
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            context = new TaskContext(opts);

            // Configure TodoItem repository for use
            repository = new Repository<TodoItem>(context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.Dispose();
        }

        [TestMethod]
        public async Task FindAsyncShouldReturnAllItemsWhenConditionTrue()
        {
            var items = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Title = "Foo",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(5),
                    IsCompleted = false
                },

                new TodoItem
                {
                    Id = 2,
                    Title = "Bar",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(7),
                    IsCompleted = true
                },

                new TodoItem
                {
                    Id = 3,
                    Title = "Baz",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(11),
                    IsCompleted = false
                },
            };

            await context.AddRangeAsync(items);
            await context.SaveChangesAsync();

            // Attempt to retrieve the items using the repository

            var result = await repository.FindAsync(x => true);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(items.All(x => result.Any(y => x == y)));
        }
    
        [TestMethod]
        public async Task FindAsyncShouldReturnAllItemsMatchingCondition()
        {
            var items = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Title = "Foo",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(5),
                    IsCompleted = false
                },

                new TodoItem
                {
                    Id = 2,
                    Title = "Bar",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(7),
                    IsCompleted = true
                },

                new TodoItem
                {
                    Id = 3,
                    Title = "Baz",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(11),
                    IsCompleted = false
                },
            };

            await context.AddRangeAsync(items);
            await context.SaveChangesAsync();

            // Attempt to retrieve the items using the repository
            var result = await repository.FindAsync(x => x.IsCompleted);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.All(x => x.Id != 1));
            Assert.IsTrue(result.Any(x => x.Id == 2));
            Assert.IsTrue(result.All(x => x.Id != 3));
        } 
    
        [TestMethod]
        public async Task SingleAsyncShouldReturnNullWhenItemDoesntExist()
        {
            var items = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Title = "Foo",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(5),
                    IsCompleted = false
                },

                new TodoItem
                {
                    Id = 2,
                    Title = "Bar",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(7),
                    IsCompleted = true
                },

                new TodoItem
                {
                    Id = 3,
                    Title = "Baz",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(11),
                    IsCompleted = false
                },
            };

            await context.AddRangeAsync(items);
            await context.SaveChangesAsync();

            // Attempt to retrieve the items using the repository
            var result = await repository.SingleAsync(4);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SingleAsyncShouldReturnRequestedItem()
        {
            var items = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Title = "Foo",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(5),
                    IsCompleted = false
                },

                new TodoItem
                {
                    Id = 2,
                    Title = "Bar",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(7),
                    IsCompleted = true
                },

                new TodoItem
                {
                    Id = 3,
                    Title = "Baz",
                    Description = "Dummy item",
                    DueDate = DateTime.Now.AddDays(11),
                    IsCompleted = false
                },
            };

            await context.AddRangeAsync(items);
            await context.SaveChangesAsync();

            var item = items.First();

            // Attempt to retrieve the items using the repository
            var result = await repository.SingleAsync(item.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(item, result);
        }
    
        [TestMethod]
        public async Task AddAsyncShouldAddANewEntry()
        {
            var newEntry = new TodoItem
            {
                Id = 123,
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now.AddDays(7),
                IsCompleted = false
            };

            await repository.AddAsync(newEntry);

            var addedEntry = await repository.SingleAsync(123);
            Assert.IsNotNull(addedEntry);
            Assert.AreEqual(newEntry, addedEntry);
        }
    
        [TestMethod]
        public async Task UpdateAsyncShouldUpdateAnEntry()
        {
            var task = new TodoItem
            {
                Id = 1,
                Title = "Old",
                Description = "Older",
                DueDate = DateTime.Now.AddDays(-2),
                IsCompleted = false
            };

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            task.Title = "New";
            task.Description = "Newer";
            task.DueDate = DateTime.Now.AddDays(7);

            var updateSuccess = await repository.UpdateAsync(task);
            Assert.IsTrue(updateSuccess);

            var ctxTask = await repository.SingleAsync(1);
            Assert.IsNotNull(ctxTask);
            Assert.AreEqual(task, ctxTask);
        }

        [TestMethod]
        public async Task DeleteAsyncShouldRemoveAnEntry()
        {
            var task = new TodoItem
            {
                Id = 123,
                Title = "Foo",
                Description = "Bar",
                DueDate = DateTime.Now.AddDays(7),
                IsCompleted = false
            };

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var deleteSuccess = await repository.DeleteAsync(task);
            Assert.IsTrue(deleteSuccess);

            var ctxTask = await repository.SingleAsync(123);
            Assert.IsNull(ctxTask);
        }
    }
}