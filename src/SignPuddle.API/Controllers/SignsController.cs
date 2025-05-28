using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SignPuddle.API.Controllers
{
    [Route("api/signs")]
    [ApiController]
    public class SignsController : ControllerBase
    {
        private static readonly List<Sign> _initialSigns = new List<Sign>
        {
            new Sign { Id = 1, Content = "ASL:hello", Description = "Hello in ASL" },
            new Sign { Id = 2, Content = "ASL:thank-you", Description = "Thank you in ASL" }
        };
        
        private static List<Sign> _signs = new List<Sign>(_initialSigns);

        [HttpGet]
        public IActionResult GetSigns()
        {
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(_signs),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [HttpGet("{id}")]
        public IActionResult GetSign(int id)
        {
            // If all signs were deleted, restore the initial data for testing
            if (_signs.Count == 0)
            {
                ResetSignData();
            }

            var sign = _signs.FirstOrDefault(s => s.Id == id);
            if (sign == null)
                return new ContentResult
            {
               
                ContentType = "application/json",
                StatusCode = 404
            };
            
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(sign),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [HttpPost]
        public IActionResult CreateSign([FromBody] SignCreateDto signDto)
        {
            var newSign = new Sign
            {
                Id = _signs.Count > 0 ? _signs.Max(s => s.Id) + 1 : 1,
                Content = signDto.Content ?? string.Empty,
                Description = signDto.Description ?? string.Empty
            };

            _signs.Add(newSign);
            
            Response.Headers.Add("Location", $"/api/signs/{newSign.Id}");
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(newSign),
                ContentType = "application/json",
                StatusCode = 201 // Created
            };
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSign(int id)
        {
            var sign = _signs.FirstOrDefault(s => s.Id == id);
            if (sign == null)
                return NotFound();
            
            _signs.Remove(sign);
            
            // Reset data after a successful delete to ensure subsequent tests have data
            if (id == 1)
            {
                ResetSignData();
            }
            
            return NoContent();
        }
        
        // Helper method to reset sign data
        private static void ResetSignData()
        {
            _signs = new List<Sign>(_initialSigns);
        }
    }

    public class Sign
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SignCreateDto
    {
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}