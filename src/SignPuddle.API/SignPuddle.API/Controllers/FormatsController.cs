using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SignPuddle.API.Controllers
{
    [Route("api/formats")]
    [ApiController]
    public class FormatsController : SignPuddleBaseController
    {
        private static List<Format> _formats = new List<Format>
        {
            new Format { Id = 1, Name = "SVG", Type = "svg" },
            new Format { Id = 2, Name = "PNG", Type = "png" }
        };

        [HttpGet]
        public IActionResult GetFormats()
        {
            return Ok((string?)JsonSerializer.Serialize(_formats));
        }

        [HttpGet("{id}")]
        public IActionResult GetFormat(int id)
        {
            var format = _formats.FirstOrDefault(f => f.Id == id);
            if (format == null)
                return NotFound();

            return Ok((string?)JsonSerializer.Serialize(format));
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

            // Updated header addition to avoid duplicate key exception
            Response.Headers.Append("Location", $"/api/formats/{newFormat.Id}");

            return CreatedAtAction(nameof(GetFormat), new { id = newFormat.Id }, (string?)JsonSerializer.Serialize(newFormat));
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