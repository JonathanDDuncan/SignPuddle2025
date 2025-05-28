using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SignPuddle.API.Controllers
{
    [Route("api/formats")]
    [ApiController]
    public class FormatsController : ControllerBase
    {
        private static List<Format> _formats = new List<Format>
        {
            new Format { Id = 1, Name = "SVG", Type = "svg" },
            new Format { Id = 2, Name = "PNG", Type = "png" }
        };

        [HttpGet]
        public IActionResult GetFormats()
        {
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(_formats),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [HttpGet("{id}")]
        public IActionResult GetFormat(int id)
        {
            var format = _formats.FirstOrDefault(f => f.Id == id);
            if (format == null)
                return NotFound();
            
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(format),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [HttpPost]
        public IActionResult CreateFormat([FromBody] FormatCreateDto formatDto)
        {
            var newFormat = new Format
            {
                Id = _formats.Count > 0 ? _formats.Max(f => f.Id) + 1 : 1,
                Name = formatDto.Name ?? string.Empty,
                Type = formatDto.Type ?? string.Empty
            };

            _formats.Add(newFormat);
            
            Response.Headers.Add("Location", $"/api/formats/{newFormat.Id}");
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(newFormat),
                ContentType = "application/json",
                StatusCode = 201 // Created
            };
        }
    }

    public class Format
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class FormatCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}