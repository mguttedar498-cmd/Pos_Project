using HMS_360_PMS.HMS_360_PMS.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult Success<T>(T data, string message = null)
      => Ok(ApiResponse<T>.SuccessResult(data, message));

        protected IActionResult Created<T>(T data, string message = null)
            => StatusCode(201, ApiResponse<T>.SuccessResult(data, message));

        protected IActionResult Fail(string message)
            => BadRequest(ApiResponse<string>.Failure(message));

        protected IActionResult NotFoundResponse(string message)
            => NotFound(ApiResponse<string>.Failure(message));
    }
}
