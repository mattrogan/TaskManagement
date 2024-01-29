using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server.UnitOfWork;

namespace Server.Controllers;

public class BaseController : ControllerBase
{
    internal readonly IUnitOfWork unitOfWork;
    internal readonly IMapper mapper;
    internal readonly ILogger logger;   

    protected BaseController()
    {
    }

    public BaseController(IUnitOfWork unitOfWork, ILogger<BaseController> logger, IMapper mapper)
        : this()
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
        this.mapper = mapper;
    }
}

public class ControllerConstants
{
    public const string POSTMODEL_NULL_ERROR = "The request body is empty or not in the expected format.";
}