using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.UnitOfWork;
using TaskManagement.Server.Data;
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
    }
}