using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignPuddle.API.Models;
using SignPuddle.API.Services;
using System.Security.Claims;

namespace SignPuddle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignController : ControllerBase
    {
        private readonly ISignService _signService;
        private readonly IRenderService _renderService;

        public SignController(ISignService signService, IRenderService renderService)
        {
            _signService = signService;
            _renderService = renderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSigns()
        {
            var signs = await _signService.GetAllSignsAsync();
            return Ok(signs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSignById(int id)
        {
            var sign = await _signService.GetSignByIdAsync(id);
            if (sign == null)
            {
                return NotFound();
            }
            return Ok(sign);
        }

        [HttpGet("dictionary/{dictionaryId}")]
        public async Task<IActionResult> GetSignsByDictionary(int dictionaryId)
        {
            var signs = await _signService.GetSignsByDictionaryAsync(dictionaryId);
            return Ok(signs);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchSigns([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Search term cannot be empty");
            }
            
            var signs = await _signService.SearchSignsByGlossAsync(term);
            return Ok(signs);
        }

        [HttpGet("{id}/svg")]
        public async Task<IActionResult> GetSignSvg(int id)
        {
            var sign = await _signService.GetSignByIdAsync(id);
            if (sign == null)
            {
                return NotFound();
            }
            
            var svg = _renderService.RenderSignSvg(sign.Fsw);
            return Content(svg, "image/svg+xml");
        }

        [HttpGet("{id}/png")]
        public async Task<IActionResult> GetSignPng(int id)
        {
            var sign = await _signService.GetSignByIdAsync(id);
            if (sign == null)
            {
                return NotFound();
            }
            
            var svg = _renderService.RenderSignSvg(sign.Fsw);
            var pngBytes = _renderService.GeneratePngImage(svg);
            return File(pngBytes, "image/png");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSign([FromBody] Sign sign)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var createdSign = await _signService.CreateSignAsync(sign, userId);
            return CreatedAtAction(nameof(GetSignById), new { id = createdSign.Id }, createdSign);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSign(int id, [FromBody] Sign sign)
        {
            if (id != sign.Id)
            {
                return BadRequest("ID mismatch");
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var updatedSign = await _signService.UpdateSignAsync(sign, userId);
            if (updatedSign == null)
            {
                return NotFound();
            }
            
            return Ok(updatedSign);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSign(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _signService.DeleteSignAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }
            
            return NoContent();
        }
    }
}