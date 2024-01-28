using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Server.UnitOfWork;

namespace Server.Controllers;

public class BaseController<T> : ControllerBase
    where T : class
{
    internal readonly IUnitOfWork unitOfWork;
    internal readonly IMapper mapper;
    internal readonly ILogger logger;   

    protected BaseController()
    {
    }

    public BaseController(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper)
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