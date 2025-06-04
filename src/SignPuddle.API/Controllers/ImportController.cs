using Microsoft.AspNetCore.Mvc;
using SignPuddle.API.Models;
using SignPuddle.API.Services;

namespace SignPuddle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ISpmlImportService _spmlImportService;

        public ImportController(ISpmlImportService spmlImportService)
        {
            _spmlImportService = spmlImportService;
        }

        /// <summary>
        /// Import a dictionary from an SPML file
        /// </summary>
        /// <param name="file">The SPML file to import</param>
        /// <param name="ownerId">Optional owner ID for the imported dictionary</param>
        /// <returns>Import result with dictionary and signs data</returns>
        [HttpPost("spml")]
        public async Task<ActionResult<SpmlImportResult>> ImportSpmlFile(
            IFormFile file, 
            [FromQuery] string? ownerId = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            if (!file.FileName.EndsWith(".spml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be an SPML file");
            }

            try
            {
                // Read file content
                using var reader = new StreamReader(file.OpenReadStream());
                var xmlContent = await reader.ReadToEndAsync();

                // Parse SPML
                var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

                // Convert to dictionary
                var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument, ownerId);

                // For demo purposes, we'll use a mock dictionary ID
                // In a real implementation, you'd save the dictionary first and get the ID
                var mockDictionaryId = 1;
                var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, mockDictionaryId);

                var result = new SpmlImportResult
                {
                    Dictionary = dictionary,
                    Signs = signs,
                    OriginalPuddleId = spmlDocument.PuddleId,
                    ImportedAt = DateTime.UtcNow,
                    TotalEntries = spmlDocument.Entries.Count,
                    ValidSigns = signs.Count
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error importing SPML file: {ex.Message}");
            }
        }

        /// <summary>
        /// Preview an SPML file without importing it
        /// </summary>
        /// <param name="file">The SPML file to preview</param>
        /// <returns>Preview information about the SPML file</returns>
        [HttpPost("spml/preview")]
        public async Task<ActionResult<SpmlPreview>> PreviewSpmlFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            if (!file.FileName.EndsWith(".spml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be an SPML file");
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var xmlContent = await reader.ReadToEndAsync();

                var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

                var preview = new SpmlPreview
                {
                    DictionaryName = spmlDocument.DictionaryName,
                    Type = spmlDocument.Type,
                    PuddleId = spmlDocument.PuddleId,
                    Created = spmlDocument.Created,
                    Modified = spmlDocument.Modified,
                    TotalEntries = spmlDocument.Entries.Count,
                    EntriesWithGloss = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Gloss)),
                    EntriesWithText = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Text)),
                    EntriesWithVideo = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Video)),
                    EntriesWithSource = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Source)),
                    SampleEntries = spmlDocument.Entries.Take(5).Select(e => new SpmlEntryPreview
                    {
                        Id = e.Id,
                        FswNotation = e.FswNotation,
                        Gloss = e.Gloss,
                        User = e.User,
                        Created = e.Created
                    }).ToList()
                };

                return Ok(preview);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error previewing SPML file: {ex.Message}");
            }
        }
    }

    public class SpmlImportResult
    {
        public Dictionary Dictionary { get; set; } = new Dictionary();
        public List<SignPuddle.API.Models.Sign> Signs { get; set; } = new List<SignPuddle.API.Models.Sign>();
        public int OriginalPuddleId { get; set; }
        public DateTime ImportedAt { get; set; }
        public int TotalEntries { get; set; }
        public int ValidSigns { get; set; }
    }

    public class SpmlPreview
    {
        public string? DictionaryName { get; set; }
        public string Type { get; set; } = string.Empty;
        public int PuddleId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int TotalEntries { get; set; }
        public int EntriesWithGloss { get; set; }
        public int EntriesWithText { get; set; }
        public int EntriesWithVideo { get; set; }
        public int EntriesWithSource { get; set; }
        public List<SpmlEntryPreview> SampleEntries { get; set; } = new List<SpmlEntryPreview>();
    }

    public class SpmlEntryPreview
    {
        public int Id { get; set; }
        public string? FswNotation { get; set; }
        public string? Gloss { get; set; }
        public string User { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
