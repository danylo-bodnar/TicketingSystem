using Microsoft.AspNetCore.Mvc;

public class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.GetValueOrThrow());
    }

    protected IActionResult HandleFailure<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return BadRequest("Expected failure but got success.");

        return BadRequest(result.Error);
    }

    protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object routeValues)
    {
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(actionName, routeValues, result.GetValueOrThrow());
    }
}