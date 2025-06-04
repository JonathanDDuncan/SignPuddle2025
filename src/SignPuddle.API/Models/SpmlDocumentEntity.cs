using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SignPuddle.API.Models
{
    /// <summary>
    /// CosmosDB entity for storing SPML documents
    /// Combines the original SPML structure with CosmosDB requirements
    /// </summary>
    public class SpmlDocumentEntity
    {
        /// <summary>
        /// Unique identifier for CosmosDB (partition key)
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Partition key for CosmosDB (uses the SPML type)
        /// </summary>
        [JsonPropertyName("partitionKey")]
        public string PartitionKey { get; set; } = string.Empty;

        /// <summary>
        /// Document type for CosmosDB queries
        /// </summary>
        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; } = "spml";

        /// <summary>
        /// The original SPML document data
        /// </summary>
        [JsonPropertyName("spmlDocument")]
        public SpmlDocument SpmlDocument { get; set; } = new();

        /// <summary>
        /// The original XML content for export purposes
        /// </summary>
        [JsonPropertyName("originalXml")]
        public string OriginalXml { get; set; } = string.Empty;

        /// <summary>
        /// User ID who uploaded/owns this document
        /// </summary>
        [JsonPropertyName("ownerId")]
        public string? OwnerId { get; set; }

        /// <summary>
        /// When this document was saved to CosmosDB
        /// </summary>
        [JsonPropertyName("savedAt")]
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this document was last updated in CosmosDB
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional description or notes about this SPML document
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Tags for categorization and search
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();        /// <summary>
        /// Metadata for additional information - stored as serialized JSON string to avoid EF Core issues
        /// </summary>
        [JsonPropertyName("metadataJson")]
        public string MetadataJson { get; set; } = "{}";

        /// <summary>
        /// Helper property to get the SPML type from embedded document
        /// </summary>
        [JsonIgnore]
        public string SpmlType => SpmlDocument?.Type ?? string.Empty;

        /// <summary>
        /// Helper property to get the puddle ID from embedded document
        /// </summary>
        [JsonIgnore]
        public int PuddleId => SpmlDocument?.PuddleId ?? 0;

        /// <summary>
        /// Helper property to get the dictionary name from embedded document
        /// </summary>
        [JsonIgnore]
        public string? DictionaryName => SpmlDocument?.DictionaryName;

        /// <summary>
        /// Helper property to get the number of entries
        /// </summary>
        [JsonIgnore]
        public int EntryCount => SpmlDocument?.Entries?.Count ?? 0;

        /// <summary>
        /// Initialize partition key based on SPML type
        /// </summary>
        public void InitializePartitionKey()
        {
            PartitionKey = !string.IsNullOrWhiteSpace(SpmlType) ? SpmlType : "unknown";
        }

        /// <summary>
        /// Convert back to original SpmlDocument for processing
        /// </summary>
        public SpmlDocument ToSpmlDocument()
        {
            return SpmlDocument;
        }

        /// <summary>
        /// Create from SpmlDocument and XML content
        /// </summary>
        public static SpmlDocumentEntity FromSpmlDocument(SpmlDocument spmlDocument, string originalXml, string? ownerId = null)
        {
            if (spmlDocument == null)
                throw new ArgumentNullException(nameof(spmlDocument), "SPML document cannot be null.");
            if (originalXml == null)
                throw new ArgumentNullException(nameof(originalXml), "Original XML content cannot be null.");
            if (string.IsNullOrWhiteSpace(originalXml))
                throw new ArgumentException("Original XML content cannot be empty or whitespace.", nameof(originalXml));

            var entity = new SpmlDocumentEntity
            {
                SpmlDocument = spmlDocument,
                OriginalXml = originalXml,
                OwnerId = ownerId,
                Description = $"SPML Dictionary: {spmlDocument.DictionaryName ?? "Unknown"}",
                Tags = new List<string>()
            };

            if (!string.IsNullOrEmpty(spmlDocument.Type))
            {
                entity.Tags.Add(spmlDocument.Type);
            }

            entity.InitializePartitionKey();
            var entriesCount = spmlDocument.Entries != null ? spmlDocument.Entries.Count : 0;
            var metadata = new Dictionary<string, object>
            {
                ["entriesCount"] = entriesCount,
                ["puddleId"] = spmlDocument.PuddleId,
                ["originalCreated"] = spmlDocument.Created,
                ["originalModified"] = spmlDocument.Modified
            };
            entity.MetadataJson = System.Text.Json.JsonSerializer.Serialize(metadata);

            return entity;
        }

        /// <summary>
        /// Get metadata dictionary from JSON string
        /// </summary>
        public Dictionary<string, object> GetMetadata()
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Set a metadata value
        /// </summary>
        public void SetMetadataValue(string key, object value)
        {
            var metadata = GetMetadata();
            metadata[key] = value;
            MetadataJson = JsonSerializer.Serialize(metadata);
        }

        /// <summary>
        /// Get a metadata value
        /// </summary>
        public T? GetMetadataValue<T>(string key, T? defaultValue = default)
        {
            var metadata = GetMetadata();
            if (metadata.TryGetValue(key, out var value))
            {
                try
                {
                    // Try to convert the JSON element to the requested type
                    if (value is JsonElement element)
                    {
                        return element.Deserialize<T>();
                    }
                    return (T)value;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}
