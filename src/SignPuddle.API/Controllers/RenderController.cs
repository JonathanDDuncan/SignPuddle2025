using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

namespace SignPuddle.API.Controllers
{
    [Route("api/render")]
    [ApiController]
    public class RenderController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetRender([FromQuery] string content = "default", [FromQuery] string format = "svg")
        {
            // In a real implementation, this would render the sign content in the specified format
            // For now, we just return a mock response

            var renderResponse = new
            {
                Content = content,
                Format = format,
                RenderedContent = $"<svg>{content}</svg>", // Mock SVG content
                Timestamp = DateTime.UtcNow
            };

            // Return a ContentResult instead of using Ok() to avoid serialization issues
            return Ok((string?)JsonSerializer.Serialize(renderResponse));
        }
    }
}

