using Microsoft.AspNetCore.Mvc;
using SignPuddle.API.Services;

namespace SignPuddle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SymbolController : ControllerBase
    {
        private readonly ISymbolService _symbolService;
        private readonly IRenderService _renderService;

        public SymbolController(ISymbolService symbolService, IRenderService renderService)
        {
            _symbolService = symbolService;
            _renderService = renderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSymbols()
        {
            var symbols = await _symbolService.GetAllSymbolsAsync();
            return Ok(symbols);
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetSymbolByKey(string key)
        {
            var symbol = await _symbolService.GetSymbolByKeyAsync(key);
            if (symbol == null)
            {
                return NotFound();
            }
            return Ok(symbol);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetSymbolsByCategory(string category)
        {
            var symbols = await _symbolService.GetSymbolsByCategoryAsync(category);
            return Ok(symbols);
        }

        [HttpGet("group/{group}")]
        public async Task<IActionResult> GetSymbolsByGroup(string group)
        {
            var symbols = await _symbolService.GetSymbolsByGroupAsync(group);
            return Ok(symbols);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _symbolService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("category/{category}/groups")]
        public async Task<IActionResult> GetGroupsByCategory(string category)
        {
            var groups = await _symbolService.GetGroupsByCategoryAsync(category);
            return Ok(groups);
        }

        [HttpGet("{key}/svg")]
        public async Task<IActionResult> GetSymbolSvg(string key)
        {
            var symbol = await _symbolService.GetSymbolByKeyAsync(key);
            if (symbol == null)
            {
                return NotFound();
            }

            var svg = _renderService.RenderSymbolSvg(key);
            return Content(svg, "image/svg+xml");
        }

        [HttpGet("{key}/png")]
        public async Task<IActionResult> GetSymbolPng(string key)
        {
            var symbol = await _symbolService.GetSymbolByKeyAsync(key);
            if (symbol == null)
            {
                return NotFound();
            }

            var svg = _renderService.RenderSymbolSvg(key);
            var pngBytes = _renderService.GeneratePngImage(svg);
            return File(pngBytes, "image/png");
        }
    }
}