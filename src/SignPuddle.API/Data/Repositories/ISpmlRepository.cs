using SignPuddle.API.Models;

namespace SignPuddle.API.Data.Repositories
{
    /// <summary>
    /// Repository interface for SPML document persistence in CosmosDB
    /// </summary>
    public interface ISpmlRepository
    {
        /// <summary>
        /// Save an SPML document to CosmosDB
        /// </summary>
        /// <param name="spmlDocument">The SPML document to save</param>
        /// <returns>The saved SPML document with assigned ID</returns>
        Task<SpmlDocumentEntity> SaveSpmlDocumentAsync(SpmlDocumentEntity spmlDocument);

        /// <summary>
        /// Save an SPML document entity (alternative method name for consistency)
        /// </summary>
        /// <param name="entity">The SPML document entity to save</param>
        /// <returns>The saved SPML document entity with assigned ID</returns>
        Task<SpmlDocumentEntity> SaveAsync(SpmlDocumentEntity entity);

        /// <summary>
        /// Get an SPML document by ID
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>The SPML document or null if not found</returns>
        Task<SpmlDocumentEntity?> GetSpmlDocumentByIdAsync(string id);

        /// <summary>
        /// Get all SPML documents
        /// </summary>
        /// <returns>List of all SPML documents</returns>
        Task<IEnumerable<SpmlDocumentEntity>> GetAllSpmlDocumentsAsync();

        /// <summary>
        /// Get SPML documents by type (e.g., "sgn")
        /// </summary>
        /// <param name="type">The SPML type</param>
        /// <returns>List of SPML documents of the specified type</returns>
        Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByTypeAsync(string type);

        /// <summary>
        /// Get SPML documents by puddle ID
        /// </summary>
        /// <param name="puddleId">The puddle ID</param>
        /// <returns>List of SPML documents with the specified puddle ID</returns>
        Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByPuddleIdAsync(int puddleId);

        /// <summary>
        /// Get SPML documents by owner ID
        /// </summary>
        /// <param name="ownerId">The owner ID</param>
        /// <returns>List of SPML documents owned by the specified user</returns>
        Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByOwnerAsync(string ownerId);

        /// <summary>
        /// Update an SPML document
        /// </summary>
        /// <param name="spmlDocument">The updated SPML document</param>
        /// <returns>The updated SPML document</returns>
        Task<SpmlDocumentEntity?> UpdateSpmlDocumentAsync(SpmlDocumentEntity spmlDocument);

        /// <summary>
        /// Delete an SPML document by ID
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        Task<bool> DeleteSpmlDocumentAsync(string id);

        /// <summary>
        /// Export SPML document back to XML format
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>The SPML document as XML string</returns>
        Task<string?> ExportSpmlDocumentAsXmlAsync(string id);

        /// <summary>
        /// Get statistics about SPML documents
        /// </summary>
        /// <returns>Statistics about stored SPML documents</returns>
        Task<SpmlStats> GetSpmlDocumentStatsAsync();
    }

    /// <summary>
    /// Statistics about SPML documents in the repository
    /// </summary>
    public class SpmlStats
    {
        public int TotalDocuments { get; set; }
        public int TotalEntries { get; set; }
        public Dictionary<string, int> DocumentsByType { get; set; } = new();
        public Dictionary<string, int> DocumentsByOwner { get; set; } = new();
    }
}
