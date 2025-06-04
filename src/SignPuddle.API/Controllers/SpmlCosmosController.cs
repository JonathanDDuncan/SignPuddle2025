using Microsoft.AspNetCore.Mvc;
using SignPuddle.API.Models;
using SignPuddle.API.Services;
using SignPuddle.API.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace SignPuddle.API.Controllers
{
    /// <summary>
    /// Controller for SPML CosmosDB operations
    /// Provides endpoints for saving, retrieving, and managing SPML documents in CosmosDB
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SpmlCosmosController : ControllerBase
    {
        private readonly ISpmlPersistenceService _spmlPersistenceService;

        public SpmlCosmosController(ISpmlPersistenceService spmlPersistenceService)
        {
            _spmlPersistenceService = spmlPersistenceService;
        }        /// <summary>
        /// Import and save an SPML file to CosmosDB
        /// </summary>
        /// <param name="file">The SPML file to import</param>
        /// <param name="ownerId">Optional owner ID for the document</param>
        /// <param name="description">Optional description for the document</param>
        /// <param name="tags">Optional comma-separated tags for categorization</param>
        /// <returns>Import result with CosmosDB document information</returns>
        [HttpPost("import")]
        public async Task<ActionResult<SpmlImportToCosmosResult>> ImportSpml( // Return type changed
            [Required] IFormFile file,
            [FromQuery] string? ownerId = null,
            [FromQuery] string? description = null,
            [FromQuery] string? tags = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new SpmlImportToCosmosResult
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            if (!file.FileName.EndsWith(".spml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new SpmlImportToCosmosResult
                {
                    Success = false,
                    Message = "File must be an SPML file (.spml extension required)"
                });
            }

            try
            {
                // Read file content
                using var reader = new StreamReader(file.OpenReadStream());
                var xmlContent = await reader.ReadToEndAsync();

                // Parse tags if provided
                var tagList = !string.IsNullOrEmpty(tags) 
                    ? tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim())
                           .Where(t => !string.IsNullOrEmpty(t))
                           .ToList()
                    : null;

                // Import and save to CosmosDB
                var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(
                    xmlContent, 
                    ownerId, 
                    description, 
                    tagList);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SpmlImportToCosmosResult
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    ErrorMessage = ex.Message // Changed from Error = ex
                });
            }
        }

        /// <summary>
        /// Get an SPML document from CosmosDB by ID
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>The SPML document entity or 404 if not found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<SpmlDocumentEntity>> GetSpmlDocument(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ProblemDetails { Title = "Document ID is required", Status = StatusCodes.Status400BadRequest });
            }

            var document = await _spmlPersistenceService.GetSpmlDocumentAsync(id);
            if (document == null)
            {
                return NotFound(new ProblemDetails { Title = $"SPML document with ID '{id}' not found", Status = StatusCodes.Status404NotFound });
            }

            return Ok(document);
        }

        /// <summary>
        /// Get all SPML documents from CosmosDB
        /// </summary>
        /// <returns>List of all SPML documents</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpmlDocumentEntity>>> GetAllSpmlDocuments()
        {
            var documents = await _spmlPersistenceService.GetAllSpmlDocumentsAsync();
            return Ok(documents);
        }

        /// <summary>
        /// Get SPML documents by owner ID
        /// </summary>
        /// <param name="ownerId">The owner ID</param>
        /// <returns>List of SPML documents owned by the specified user</returns>
        [HttpGet("owner/{ownerId}")]
        public async Task<ActionResult<IEnumerable<SpmlDocumentEntity>>> GetSpmlDocumentsByOwner(string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                return BadRequest(new ProblemDetails { Title = "Owner ID is required", Status = StatusCodes.Status400BadRequest });
            }

            var documents = await _spmlPersistenceService.GetSpmlDocumentsByOwnerAsync(ownerId);
            return Ok(documents);
        }        /// <summary>
        /// Export an SPML document from CosmosDB as XML
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>The SPML document as XML content</returns>
        [HttpGet("{id}/export")]
        [Produces("application/xml", "text/xml")]
        public async Task<ActionResult> ExportSpmlDocument(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ProblemDetails { Title = "Document ID is required", Status = StatusCodes.Status400BadRequest });
            }

            var xmlContent = await _spmlPersistenceService.ExportSpmlDocumentAsXmlAsync(id);
            if (xmlContent == null)
            {
                return NotFound(new ProblemDetails { Title = $"SPML document with ID '{id}' not found", Status = StatusCodes.Status404NotFound });
            }

            var fileName = $"spml_export_{id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.spml";
            return File(System.Text.Encoding.UTF8.GetBytes(xmlContent), "application/xml", fileName);
        }        /// <summary>
        /// Convert a CosmosDB SPML document to Dictionary and Signs entities
        /// </summary>
        /// <param name="id">The document ID or document entity</param>
        /// <returns>Conversion result with Dictionary and Signs data</returns>
        [HttpPost("{id}/convert")]
        public async Task<ActionResult<SpmlConversionResult>> ConvertToEntities(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ProblemDetails { Title = "Document ID is required", Status = StatusCodes.Status400BadRequest });
            }

            var document = await _spmlPersistenceService.GetSpmlDocumentAsync(id);
            if (document == null)
            {
                return NotFound(new ProblemDetails { Title = $"SPML document with ID '{id}' not found", Status = StatusCodes.Status404NotFound });
            }

            try
            {
                var conversionResult = await _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(document);
                return Ok(conversionResult);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails 
                { 
                    Title = "Failed to convert SPML document", 
                    Detail = ex.Message, 
                    Status = StatusCodes.Status500InternalServerError 
                });
            }
        }
          /// <summary>
        /// Convert a CosmosDB SPML document to Dictionary and Signs entities
        /// </summary>
        /// <param name="document">The SPML document entity</param>
        /// <returns>Conversion result with Dictionary and Signs data</returns>
        [HttpPost("convert")]
        public async Task<ActionResult<SpmlConversionResult>> ConvertToEntitiesFromBody([FromBody] SpmlDocumentEntity document)
        {
            if (document == null)
            {
                return BadRequest(new ProblemDetails { Title = "Document cannot be null", Status = StatusCodes.Status400BadRequest });
            }

            try
            {
                var conversionResult = await _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(document);
                return Ok(conversionResult);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails 
                { 
                    Title = "Failed to convert SPML document", 
                    Detail = ex.Message, 
                    Status = StatusCodes.Status500InternalServerError 
                });
            }
        }

        /// <summary>
        /// Delete an SPML document from CosmosDB
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSpmlDocument(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new ProblemDetails { Title = "Document ID is required", Status = StatusCodes.Status400BadRequest });
            }

            var success = await _spmlPersistenceService.DeleteSpmlDocumentAsync(id);
            if (!success)
            {
                return NotFound(new ProblemDetails { Title = $"SPML document with ID '{id}' not found", Status = StatusCodes.Status404NotFound });
            }

            return Ok(new { Message = $"SPML document '{id}' deleted successfully" });
        }        /// <summary>
        /// Get statistics about SPML documents in CosmosDB
        /// </summary>
        /// <returns>Statistics including document counts and types</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<SpmlStats>> GetStats()
        {
            var stats = await _spmlPersistenceService.GetSpmlDocumentStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Health check endpoint for SPML CosmosDB operations
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        public ActionResult GetHealth()
        {
            return Ok(new 
            { 
                Status = "Healthy", 
                Service = "SPML CosmosDB Service", 
                Timestamp = DateTime.UtcNow 
            });
        }
    }
}
