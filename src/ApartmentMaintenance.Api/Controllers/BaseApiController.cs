using Microsoft.AspNetCore.Mvc;
using ApartmentMaintenance.Application.Common;

namespace ApartmentMaintenance.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected ActionResult<ApiResponse<T>> FailResponse<T>(string message, List<string>? errors = null, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<T>.Fail(message, errors));
    }
}
