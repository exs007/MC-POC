using Microsoft.AspNetCore.Mvc;
using MS.API.Filters;

namespace MS.API.Controllers
{
    /// <summary>
    /// Base API controller, includes fluent validation and formatting validation errors in the common structure
    /// </summary>
    [ApiController]
    [FluentValidationFilter]
    [ModelStateValidationFilter]
    public abstract class APIBaseController : ControllerBase
    {
    }
}