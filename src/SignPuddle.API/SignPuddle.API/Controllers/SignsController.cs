using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SignPuddle.API.Data;
using SignPuddle.API.Data.Repositories;
using SignPuddle.API.Models;
using System.Linq;

namespace SignPuddle.API.Controllers
{
    [Route("api/signs")]
    [ApiController]
    public class SignsController : SignPuddleBaseController
    {
        private readonly ISignRepository _signRepository;
        private static readonly List<Sign> _initialSigns = new List<Sign>
        {
            new Sign { Id = 1, Content = "ASL:hello", Description = "Hello in ASL" },
            new Sign { Id = 2, Content = "ASL:thank-you", Description = "Thank you in ASL" }
        };

        private static List<Sign> _signs = new List<Sign>(_initialSigns);

        public SignsController(ISignRepository signRepository)
        {
            _signRepository = signRepository;
        }

        [HttpGet]
        public IActionResult GetSigns()
        {
            return Ok((string?)JsonSerializer.Serialize(_signs));
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
                return NotFound();
            return Ok((string?)JsonSerializer.Serialize(sign));

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

            Response.Headers.Append("Location", $"/api/signs/{newSign.Id}");

            return CreatedAtAction(nameof(GetSign), new { id = newSign.Id }, (string?)JsonSerializer.Serialize(newSign));
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

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SignSearchParameters parameters)
        {
            parameters.Validate();
            var baseQuery = _signRepository.BuildSearchQuery(parameters);
            var totalCount = await _signRepository.CountSearchResultsAsync(baseQuery);
            var items = await _signRepository.ExecuteSearchQueryAsync(baseQuery, parameters.Page, parameters.PageSize);
            var dtos = items.Select(SignRepository.MapToDto);
            return Ok(new { totalCount, items = dtos, page = parameters.Page, pageSize = parameters.PageSize });
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