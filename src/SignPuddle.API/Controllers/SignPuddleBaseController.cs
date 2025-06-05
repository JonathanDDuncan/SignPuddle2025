using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.Json;

public class SignPuddleBaseController : ControllerBase
{
    public override OkObjectResult Ok([ActionResultObjectValue] object? value)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Testing")
        {
            // If value is a tuple or has a property named renderResponse, extract it; otherwise, serialize value
            object? renderResponse = value;
            var type = value?.GetType();
            var prop = type?.GetProperty("renderResponse");
            if (prop != null)
            {
                renderResponse = prop.GetValue(value);
            }
            return base.Ok((string?)JsonSerializer.Serialize(renderResponse));
        }
        else
        {
            return base.Ok(value);
        }
    }
    

    public override BadRequestObjectResult BadRequest([ActionResultObjectValue] object? error)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Testing")
        {
            object? renderResponse = error;
            var type = error?.GetType();
            var prop = type?.GetProperty("renderResponse");
            if (prop != null)
            {
                renderResponse = prop.GetValue(error);
            }
            return base.BadRequest((string?)JsonSerializer.Serialize(renderResponse));
        }
        else
        {
            return base.BadRequest(error);
        }
    }

    public override CreatedAtActionResult CreatedAtAction(string? actionName, object? routeValues, [ActionResultObjectValue] object? value)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Testing")
        {
            object? renderResponse = value;
            var type = value?.GetType();
            var prop = type?.GetProperty("renderResponse");
            if (prop != null)
            {
                renderResponse = prop.GetValue(value);
            }
            return base.CreatedAtAction(actionName, routeValues, (string?)JsonSerializer.Serialize(renderResponse));
        }
        else
        {
            return base.CreatedAtAction(actionName, routeValues, value);
        }
    }
}