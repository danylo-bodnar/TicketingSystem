using Microsoft.AspNetCore.Mvc;
using Ticketing.Application.Common;

public class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.GetValueOrThrow());
    }

    protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object routeValues)
    {
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(actionName, routeValues, result.GetValueOrThrow());
    }
}