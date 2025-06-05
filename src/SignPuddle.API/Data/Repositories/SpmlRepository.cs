using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SignPuddle.API.Data.Repositories
{
    /// <summary>
    /// Repository implementation for SPML document persistence in CosmosDB
    /// </summary>
    public class SpmlRepository : ISpmlRepository
    {
        private readonly ApplicationDbContext _context;

        public SpmlRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SpmlDocumentEntity> SaveSpmlDocumentAsync(SpmlDocumentEntity spmlDocument)
        {
            if (spmlDocument == null)
                throw new ArgumentNullException(nameof(spmlDocument));

            // Ensure partition key is set
            spmlDocument.InitializePartitionKey();
            spmlDocument.SavedAt = DateTime.UtcNow;
            spmlDocument.UpdatedAt = DateTime.UtcNow;

            _context.SpmlDocuments.Add(spmlDocument);
            await _context.SaveChangesAsync();

            return spmlDocument;
        }

        /// <summary>
        /// Save an SPML document entity (alternative method name for consistency)
        /// </summary>
        public async Task<SpmlDocumentEntity> SaveAsync(SpmlDocumentEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return await SaveSpmlDocumentAsync(entity);
        }

        public async Task<SpmlDocumentEntity?> GetSpmlDocumentByIdAsync(string id)
        {
            return await _context.SpmlDocuments
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<SpmlDocumentEntity>> GetAllSpmlDocumentsAsync()
        {
            return await _context.SpmlDocuments
                .OrderByDescending(d => d.SavedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByTypeAsync(string type)
        {
            // Using PartitionKey for filtering by type, assuming PartitionKey is set based on SpmlDocument.Type
            return await _context.SpmlDocuments
                .Where(d => d.PartitionKey == type) // Changed to use PartitionKey
                .OrderByDescending(d => d.SavedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByPuddleIdAsync(int puddleId)
        {
            // EF Core might struggle with s.SpmlDocument.PuddleId.
            // Fetching all and filtering in memory as a workaround.
            // This is not ideal for performance with large datasets.
            // Consider adding a mapped PuddleId property to SpmlDocumentEntity if this is a common query.
            var allDocuments = await _context.SpmlDocuments
                .OrderByDescending(d => d.SavedAt)
                .ToListAsync();
            return allDocuments.Where(d => d.SpmlDocument.PuddleId == puddleId);
        }

        public async Task<SpmlDocumentEntity?> UpdateSpmlDocumentAsync(SpmlDocumentEntity spmlDocument)
        {
            if (spmlDocument == null)
                throw new ArgumentNullException(nameof(spmlDocument));

            var existingDocument = await GetSpmlDocumentByIdAsync(spmlDocument.Id);
            if (existingDocument == null)
            {
                return null;
            }

            // Update the entity
            spmlDocument.UpdatedAt = DateTime.UtcNow;
            spmlDocument.InitializePartitionKey();

            _context.Entry(existingDocument).CurrentValues.SetValues(spmlDocument);
            await _context.SaveChangesAsync();

            return spmlDocument;
        }

        public async Task<bool> DeleteSpmlDocumentAsync(string id)
        {
            var document = await GetSpmlDocumentByIdAsync(id);
            if (document == null)
            {
                return false;
            }

            _context.SpmlDocuments.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> ExportSpmlDocumentAsXmlAsync(string id)
        {
            var document = await GetSpmlDocumentByIdAsync(id);
            if (document == null)
            {
                return null;
            }

            // If we have the original XML, return it
            if (!string.IsNullOrEmpty(document.OriginalXml))
            {
                return document.OriginalXml;
            }

            // Otherwise, serialize the SpmlDocument back to XML
            return SerializeSpmlDocumentToXml(document.SpmlDocument);
        }

        /// <summary>
        /// Serialize an SpmlDocument back to XML format
        /// </summary>
        private static string SerializeSpmlDocumentToXml(SpmlDocument spmlDocument)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SpmlDocument));
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n",
                    Encoding = Encoding.UTF8,
                    OmitXmlDeclaration = false
                };

                using var stringWriter = new StringWriter();
                using var xmlWriter = XmlWriter.Create(stringWriter, settings);

                // Add DOCTYPE declaration for DTD compliance
                xmlWriter.WriteDocType("spml", null, "http://www.signpuddle.net/spml_1.6.dtd", null);

                serializer.Serialize(xmlWriter, spmlDocument);
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize SPML document to XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get SPML documents by owner ID
        /// </summary>
        public async Task<IEnumerable<SpmlDocumentEntity>> GetSpmlDocumentsByOwnerAsync(string ownerId)
        {
            return await _context.SpmlDocuments
                .Where(d => d.OwnerId == ownerId)
                .OrderByDescending(d => d.SavedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Search SPML documents by dictionary name
        /// </summary>
        public async Task<IEnumerable<SpmlDocumentEntity>> SearchSpmlDocumentsByNameAsync(string searchTerm)
        {
            return await _context.SpmlDocuments
                .Where(d => d.SpmlDocument.Terms.Any(t => t.Contains(searchTerm)) ||
                           (d.Description != null && d.Description.Contains(searchTerm)))
                .OrderByDescending(d => d.SavedAt)
                .ToListAsync();
        }        /// <summary>
        /// Get SPML document statistics
        /// </summary>
        public async Task<SpmlStats> GetSpmlDocumentStatsAsync()
        {
            var totalDocuments = await _context.SpmlDocuments.CountAsync();
            
            // Use client evaluation for complex operations on owned entities
            var documents = await _context.SpmlDocuments.ToListAsync();
            var totalEntries = documents.Sum(d => d.SpmlDocument?.Entries?.Count ?? 0);
            
            var typeGroups = documents
                .GroupBy(d => d.SpmlDocument?.Type ?? "unknown")
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            var ownerGroups = documents
                .GroupBy(d => d.OwnerId ?? "unknown")
                .Select(g => new { Owner = g.Key, Count = g.Count() })
                .ToList();

            return new SpmlStats
            {
                TotalDocuments = totalDocuments,
                TotalEntries = totalEntries,
                DocumentsByType = typeGroups.ToDictionary(x => x.Type, x => x.Count),
                DocumentsByOwner = ownerGroups.ToDictionary(x => x.Owner, x => x.Count)
            };
        }
    }
}
