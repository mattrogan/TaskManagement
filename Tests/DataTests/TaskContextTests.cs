using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Server.Data;
using TaskManagement.Server.MappingProfiles;
using TaskManagement.Shared.Models;

namespace Tests.DataTests;

[TestClass]
public class TaskContextTests
{
    private TaskContext context;

    [TestInitialize]
    public void Initialise()
    {
        // Configure InMemory testing database
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var opts = new DbContextOptionsBuilder<TaskContext>()
            .UseInMemoryDatabase("TestDatabase")
            .UseInternalServiceProvider(serviceProvider)
            .Options;

        this.context = new TaskContext(opts);
    }

    [TestCleanup]
    public void Cleanup()
    {
        this.context.Dispose();
    }

    [TestMethod]
    public async Task TestDataCanBeAddedAsynchronously()
    {
        var dueDate = DateTime.Now.AddDays(5);

        var items = new List<TodoItem>
        {
            new TodoItem
            {
                Id = 1,
                Title = "Foo",
                Description = "Dummy item",
                DueDate = dueDate,
                IsCompleted = false
            },

            new TodoItem
            {
                Id = 2,
                Title = "Bar",
                Description = "Dummy item",
                DueDate = dueDate,
                IsCompleted = false
            },

            new TodoItem
            {
                Id = 3,
                Title = "Baz",
                Description = "Dummy item",
                DueDate = dueDate,
                IsCompleted = false
            },
        };

        await context.AddRangeAsync(items);
        var result = context.SaveChangesAsync();
        Assert.AreEqual(TaskStatus.RanToCompletion, result.Status);
    }

    [TestMethod]
    public async Task TestDataCanBeRetrievedAsynchronously()
    {
        var dueDate = DateTime.Now.AddDays(5);

        var todoItem = new TodoItem
        {
            Id = 123,
            Title = "Foo",
            Description = "Bar",
            DueDate = dueDate,
            IsCompleted = false
        };

        await context.AddAsync(todoItem);
        await context.SaveChangesAsync();

        var retrievedItem = await context.Set<TodoItem>().FindAsync(todoItem.Id);
        Assert.IsNotNull(retrievedItem);
        Assert.AreEqual(todoItem, retrievedItem);
    }

    [TestMethod]
    public async Task TestDataCanBeUpdated()
    {
        var dest = new TodoItem
        {
            Id = 1,
            Title = "Old",
            Description = "Desc old",
            DueDate = DateTime.Now.AddDays(-10),
            IsCompleted = false
        };

        await context.AddAsync(dest);
        await context.SaveChangesAsync();
        
        var src = new TodoItem
        {
            Id = 1,
            Title = "New",
            Description = "Desc new",
            DueDate = DateTime.Now.AddDays(5),
            IsCompleted = true
        };

        // Set up a mapper
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new TodoItemMappingProfiles())).CreateMapper();

        mapper.Map(src, dest);

        await context.SaveChangesAsync();

        // Check that the entry has been updated in the ctx too
        var result = await context.Set<TodoItem>().FindAsync(src.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(dest, result);
    }

    [TestMethod]
    public async Task TestDataCanBeDeleted()
    {
        var dueDate = DateTime.Now.AddDays(5);

        var items = new List<TodoItem>
        {
            new TodoItem
            {
                Id = 1,
                Title = "Foo",
                Description = "Dummy item",
                DueDate = dueDate,
                IsCompleted = false
            },

            new TodoItem
            {
                Id = 2,
                Title = "Bar",
                Description = "Dummy item",
                DueDate = dueDate,
                IsCompleted = false
            },

            new TodoItem
            {
                Id = 3,
                Title = "Baz",
                Description = "Dummy item",
                DueDate = dueDate,
                IsCompleted = false
            },
        };

        await context.AddRangeAsync(items);
        await context.SaveChangesAsync();

        // Delete the second item
        var deleteItem = items.Single(x => x.Id == 2);
        context.Remove(deleteItem);
        await context.SaveChangesAsync();

        // Now, get all items - is the second one gone?
        var newItems = await context.Set<TodoItem>().ToListAsync();
        Assert.IsNotNull(newItems);
        Assert.AreEqual(2, newItems.Count);
        Assert.IsTrue(newItems.Any(x => x.Id == 1));
        Assert.IsTrue(newItems.All(x => x.Id != 2));
        Assert.IsTrue(newItems.Any(x => x.Id == 3));
    }
}



