using FluentResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        var messages = result.Errors.Select(e => e.Message).ToArray();
        var combinedMessage = string.Join(", ", messages);

        var statusCode = result.Errors.First().Metadata.TryGetValue("StatusCode", out var value) 
            && value is HttpStatusCode code
                         ? (int)code
                         : 400;

        var problem = new ProblemDetails()
        {
            Title = "خطا در درخواست",
            Detail = combinedMessage,
            Status = statusCode
        };

        return StatusCode(problem.Status.Value, problem);
    }


    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        var error = result.Errors.First();

        var problem = new ProblemDetails()
        {
            Title = "خطا در درخواست",
            Detail = error.Message,
            Status = error.Metadata.TryGetValue("StatusCode", out var value) && value is HttpStatusCode code
                     ? (int)code
                     : 400
        };

        return StatusCode(problem.Status.Value, problem);
    }
}
