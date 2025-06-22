using System.Xml;
using System.Xml.Serialization;
using SignPuddle.API.Models;

namespace SignPuddle.API.Services
{
    public interface ISpmlImportService
    {
        Task<SpmlDocument> ParseSpmlAsync(string xmlContent);
        Task<SpmlDocument> ParseSpmlFromFileAsync(string filePath);
        Task<Dictionary> ConvertToDictionaryAsync(SpmlDocument spmlDocument, string? ownerId = null);
        Task<List<Sign>> ConvertToSignsAsync(SpmlDocument spmlDocument, string dictionaryId);
    }

    public class SpmlImportService : ISpmlImportService
    {
        public async Task<SpmlDocument> ParseSpmlAsync(string xmlContent)
        {
            var serializer = new XmlSerializer(typeof(SpmlDocument));

            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                ValidationType = ValidationType.None
            });

            var spmlDocument = (SpmlDocument?)serializer.Deserialize(xmlReader);

            if (spmlDocument == null)
                throw new InvalidOperationException("Failed to deserialize SPML document");

            return await Task.FromResult(spmlDocument);
        }

        public async Task<SpmlDocument> ParseSpmlFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SPML file not found: {filePath}");

            var xmlContent = await File.ReadAllTextAsync(filePath);
            return await ParseSpmlAsync(xmlContent);
        }

        public async Task<Dictionary> ConvertToDictionaryAsync(SpmlDocument spmlDocument, string? ownerId = null)
        {
            if (spmlDocument == null)
                throw new ArgumentNullException(nameof(spmlDocument));
            var dictionary = new Dictionary
            {
                Id = Guid.NewGuid().ToString(),
                PuddleId = spmlDocument.PuddleId.ToString(),
                PuddleType = spmlDocument.Type,
                Name = spmlDocument.DictionaryName ?? $"Imported Dictionary puddle {spmlDocument.PuddleId}",
                Description = $"Imported from SPML puddle {spmlDocument.PuddleId}",
                IsPublic = true,
                OwnerId = ownerId,
                Created = spmlDocument.Created,
                Updated = spmlDocument.Modified
            };

            return await Task.FromResult(dictionary);
        }
        public async Task<List<Sign>> ConvertToSignsAsync(SpmlDocument spmlDocument, string dictionaryId)
        {
            if (spmlDocument == null)
                throw new ArgumentNullException(nameof(spmlDocument));

            var signs = new List<Sign>();

            foreach (var entry in spmlDocument.Entries)
            {
                // Skip entries with invalid or missing EntryId
                if (!entry.EntryId.HasValue)
                    continue;
                // Use the helper function for FSW validation
                if (!IsValidFswSign(entry.Fsw) ||
                    entry.Gloss == null || entry.Gloss.Count == 0 || string.IsNullOrWhiteSpace(entry.Gloss[0]))
                    continue; // Skip entries without proper FSW notation or gloss

                var sign = new Sign
                {
                    DictionaryId = dictionaryId,
                    PuddleSignId = entry.EntryId.Value,
                    PuddleId = spmlDocument.PuddleId.ToString(),
                    Fsw = entry.Fsw ?? string.Empty,
                    Gloss = entry.Gloss,
                    Description = entry.Text,
                    Created = entry.Created,
                    Updated = entry.Modified,
                    CreatedBy = entry.User,
                    UpdatedBy = entry.User
                };

                signs.Add(sign);
            }

            return await Task.FromResult(signs);
        }        /// <summary>
                 /// Determines if the input string is a valid Formal SignWriting (FSW) string.
                 /// </summary>
        private static bool IsValidFswSign(string? input)
        {
            return SignPuddle.API.Models.FswValidation.IsValidFswSign(input);
        }
    }
}
