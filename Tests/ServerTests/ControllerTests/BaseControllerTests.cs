using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Server.UnitOfWork;
using TaskManagement.Shared.Models;

namespace Tests.ServerTests.ControllerTests;

public class BaseControllerTests
{
    internal Mock<IUnitOfWork> mockUnitOfWork;
    internal Mock<ILogger> mockLogger;
    internal Mock<IMapper> mockMapper;

    internal Mock<IRepository<TodoItem>> mockTaskRepository;

    [TestInitialize]
    public void Initialize()
    {
        mockUnitOfWork = new(MockBehavior.Strict);
        mockLogger = new(MockBehavior.Strict);
        mockMapper = new(MockBehavior.Strict);

        mockTaskRepository = new(MockBehavior.Strict);

        mockLogger
            .Setup(x => x.Log<It.IsAnyType>(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(), 
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

        mockUnitOfWork
            .Setup(x => x.GetRepository<TodoItem>())
            .Returns(mockTaskRepository.Object)
            .Verifiable();
    }   

    [TestCleanup]
    public void Cleanup()
    {
        mockUnitOfWork.Verify();
        mockLogger.Verify();
        mockMapper.Verify();

        mockTaskRepository.Verify();
    }     
}