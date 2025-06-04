using SignPuddle.API.Data.Repositories;
using SignPuddle.API.Models;

namespace SignPuddle.API.Services
{
    /// <summary>
    /// Enhanced SPML service interface with CosmosDB persistence capabilities
    /// </summary>
    public interface ISpmlPersistenceService
    {
        /// <summary>
        /// Save SPML document to CosmosDB
        /// </summary>
        /// <param name="spmlDocument">The SPML document to save</param>
        /// <param name="originalXml">The original XML content</param>
        /// <param name="ownerId">Optional owner ID</param>
        /// <param name="description">Optional description</param>
        /// <param name="tags">Optional tags for categorization</param>
        /// <returns>The saved SPML document entity</returns>
        Task<SpmlDocumentEntity> SaveSpmlDocumentAsync(
            SpmlDocument spmlDocument, 
            string originalXml, 
            string? ownerId = null,
            string? description = null,
            List<string>? tags = null);

        /// <summary>
        /// Import SPML file and save to CosmosDB
        /// </summary>
        /// <param name="xmlContent">The SPML XML content</param>
        /// <param name="ownerId">Optional owner ID</param>
        /// <param name="description">Optional description</param>
        /// <param name="tags">Optional tags</param>
        /// <returns>Import result with CosmosDB entity</returns>
        Task<SpmlImportToCosmosResult> ImportAndSaveSpmlAsync(
            string xmlContent,
            string? ownerId = null,
            string? description = null,
            List<string>? tags = null);

        /// <summary>
        /// Get SPML document from CosmosDB
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>The SPML document entity or null</returns>
        Task<SpmlDocumentEntity?> GetSpmlDocumentAsync(string id);

        /// <summary>
        /// Export SPML document from CosmosDB as XML
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>XML content or null if not found</returns>
        Task<string?> ExportSpmlDocumentAsXmlAsync(string id);

        /// <summary>
        /// Convert CosmosDB SPML document to Dictionary and Signs for EF Core
        /// </summary>
        /// <param name="spmlDocumentEntity">The SPML document entity from CosmosDB</param>
        /// <returns>Conversion result with Dictionary and Signs</returns>
        Task<SpmlConversionResult> ConvertSpmlDocumentToEntitiesAsync(SpmlDocumentEntity spmlDocumentEntity);

        /// <summary>
        /// Get all SPML documents from CosmosDB
        /// </summary>
        /// <returns>List of SPML document entities</returns>
        Task<IEnumerable<SpmlDocumentEntity>> GetAllSpmlDocumentsAsync();

        /// <summary>
        /// Get SPML documents by owner
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <returns>List of SPML document entities</returns>
        Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByOwnerAsync(string ownerId);

        /// <summary>
        /// Delete SPML document from CosmosDB
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteSpmlDocumentAsync(string id);        /// <summary>
        /// Get repository statistics
        /// </summary>
        /// <returns>Statistics about SPML documents</returns>
        Task<SpmlStats> GetSpmlDocumentStatsAsync();
    }

    /// <summary>
    /// Enhanced SPML service with CosmosDB persistence capabilities
    /// </summary>
    public class SpmlPersistenceService : ISpmlPersistenceService
    {
        private readonly ISpmlImportService _spmlImportService;
        private readonly ISpmlRepository _spmlRepository;

        public SpmlPersistenceService(
            ISpmlImportService spmlImportService,
            ISpmlRepository spmlRepository)
        {
            _spmlImportService = spmlImportService;
            _spmlRepository = spmlRepository;
        }        public async Task<SpmlDocumentEntity> SaveSpmlDocumentAsync(
            SpmlDocument spmlDocument, 
            string originalXml, 
            string? ownerId = null,
            string? description = null,
            List<string>? tags = null)
        {
            // Create the entity from the SPML document            // Create a new entity directly
            var entity = new SpmlDocumentEntity
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = spmlDocument.Type ?? "default",
                DocumentType = "spml",
                SpmlDocument = spmlDocument,
                OriginalXml = originalXml,
                OwnerId = ownerId,
                Description = $"SPML Dictionary: {spmlDocument.DictionaryName}",
                Tags = new List<string> { spmlDocument.Type }
            };
            
            // Add optional description and tags
            if (!string.IsNullOrEmpty(description))
            {
                entity.Description = description;
            }

            if (tags != null && tags.Count > 0)
            {
                // For tests, ensure we have exactly the right number of tags
                entity.Tags.AddRange(tags);
            }

            // Save to CosmosDB
            return await _spmlRepository.SaveSpmlDocumentAsync(entity);
        }

        public async Task<SpmlImportToCosmosResult> ImportAndSaveSpmlAsync(
            string xmlContent,
            string? ownerId = null,
            string? description = null,
            List<string>? tags = null)
        {
            try
            {
                // Parse the SPML content
                var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

                // Save to CosmosDB
                var savedEntity = await SaveSpmlDocumentAsync(
                    spmlDocument, 
                    xmlContent, 
                    ownerId, 
                    description, 
                    tags);

                // Convert to Dictionary and Signs for EF Core compatibility
                var conversionResult = await ConvertSpmlDocumentToEntitiesAsync(savedEntity);

                return new SpmlImportToCosmosResult
                {
                    Success = true,
                    SpmlDocumentEntity = savedEntity,
                    Dictionary = conversionResult.Dictionary,
                    Signs = conversionResult.Signs,
                    Message = $"Successfully imported SPML document with {savedEntity.EntryCount} entries"
                };
            }
            catch (Exception ex)
            {
                return new SpmlImportToCosmosResult
                {
                    Success = false,
                    Message = $"Failed to import SPML document: {ex.Message}",
                    Error = ex
                };
            }
        }

        public async Task<SpmlDocumentEntity?> GetSpmlDocumentAsync(string id)
        {
            return await _spmlRepository.GetSpmlDocumentByIdAsync(id);
        }

        public async Task<string?> ExportSpmlDocumentAsXmlAsync(string id)
        {
            return await _spmlRepository.ExportSpmlDocumentAsXmlAsync(id);
        }

        public async Task<SpmlConversionResult> ConvertSpmlDocumentToEntitiesAsync(SpmlDocumentEntity spmlDocumentEntity)
        {
            var spmlDocument = spmlDocumentEntity.SpmlDocument;

            // Convert to Dictionary
            var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument, spmlDocumentEntity.OwnerId);

            // Convert to Signs (use a placeholder dictionary ID since we're not saving to EF Core yet)
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, 0);

            return new SpmlConversionResult
            {
                Dictionary = dictionary,
                Signs = signs,
                SpmlDocument = spmlDocument
            };
        }

        public async Task<IEnumerable<SpmlDocumentEntity>> GetAllSpmlDocumentsAsync()
        {
            return await _spmlRepository.GetAllSpmlDocumentsAsync();
        }

        public async Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByOwnerAsync(string ownerId)
        {
            return await _spmlRepository.GetSpmlDocumentsByOwnerAsync(ownerId);
        }

        public async Task<bool> DeleteSpmlDocumentAsync(string id)
        {
            return await _spmlRepository.DeleteSpmlDocumentAsync(id);
        }        public async Task<SpmlStats> GetSpmlDocumentStatsAsync()
        {
            return await _spmlRepository.GetSpmlDocumentStatsAsync();
        }
    }

    /// <summary>
    /// Result of importing SPML to CosmosDB
    /// </summary>
    public class SpmlImportToCosmosResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SpmlDocumentEntity? SpmlDocumentEntity { get; set; }
        public Dictionary? Dictionary { get; set; }
        public List<Sign>? Signs { get; set; }
        public Exception? Error { get; set; }
    }

    /// <summary>
    /// Result of converting SPML document to EF Core entities
    /// </summary>
    public class SpmlConversionResult
    {
        public Dictionary Dictionary { get; set; } = new();
        public List<Sign> Signs { get; set; } = new();
        public SpmlDocument SpmlDocument { get; set; } = new();
    }
}
