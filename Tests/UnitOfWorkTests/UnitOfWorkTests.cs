using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.UnitOfWork;
using TaskManagement.Server.Data;
using TaskManagement.Shared.Models;

namespace Tests.UnitOfWorkTests;

[TestClass]
public class UnitOfWorkTests
{
    private TaskContext context;
    private IUnitOfWork unitOfWork;

    [TestInitialize]
    public void Initialize()
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

        // Set up full unit of work
        unitOfWork = new UnitOfWork(context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task UnitOfWorkShouldGenerateRepositories()
    {
        // Set up data
        await context.AddRangeAsync(new List<TodoItem>
        {
            new TodoItem(){ Id = 1, Title = "Foo", Description = "Foo", DueDate = DateTime.Now, IsCompleted = true },
            new TodoItem(){ Id = 2, Title = "Bar", Description = "Bar", DueDate = DateTime.Now, IsCompleted = false },
            new TodoItem(){ Id = 3, Title = "Baz", Description = "Baz", DueDate = DateTime.Now, IsCompleted = true },
        });
        await context.SaveChangesAsync();

        var todoItemRepository = unitOfWork.GetRepository<TodoItem>();
        Assert.IsNotNull(todoItemRepository);
        Assert.IsInstanceOfType(todoItemRepository, typeof(IRepository<TodoItem>));
    }

    [TestMethod]
    public async Task UnitOfWorkRepositoriesShouldWorkAsExpected()
    {
        // Set up data
        await context.AddRangeAsync(new List<TodoItem>
        {
            new TodoItem(){ Id = 1, Title = "Foo", Description = "Foo", DueDate = DateTime.Now, IsCompleted = true },
            new TodoItem(){ Id = 2, Title = "Bar", Description = "Bar", DueDate = DateTime.Now, IsCompleted = false },
            new TodoItem(){ Id = 3, Title = "Baz", Description = "Baz", DueDate = DateTime.Now, IsCompleted = true },
        });
        await context.SaveChangesAsync();

        var todoItemRepository = unitOfWork.GetRepository<TodoItem>();
        var items = await todoItemRepository.FindAsync(x => x.IsCompleted);

        Assert.IsNotNull(items);
        Assert.IsInstanceOfType(items, typeof(IEnumerable<TodoItem>));
        Assert.AreEqual(2, items.Count());
    }
}