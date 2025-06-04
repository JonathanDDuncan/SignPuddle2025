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
        Task<List<Sign>> ConvertToSignsAsync(SpmlDocument spmlDocument, int dictionaryId);
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
                Name = spmlDocument.DictionaryName ?? $"Imported Dictionary {spmlDocument.PuddleId}",
                Description = $"Imported from SPML puddle {spmlDocument.PuddleId}",
                Language = DetermineLanguageFromType(spmlDocument.Type),
                IsPublic = true,
                OwnerId = ownerId,
                Created = spmlDocument.Created,
                Updated = spmlDocument.Modified
            };

            return await Task.FromResult(dictionary);
        }        public async Task<List<Sign>> ConvertToSignsAsync(SpmlDocument spmlDocument, int dictionaryId)
        {
            if (spmlDocument == null)
                throw new ArgumentNullException(nameof(spmlDocument));

            var signs = new List<Sign>();

            foreach (var entry in spmlDocument.Entries)
            {
                // Use the helper function for FSW validation
                if (!IsValidFsw(entry.FswNotation) ||
                    string.IsNullOrWhiteSpace(entry.Gloss))
                    continue; // Skip entries without proper FSW notation or gloss

                var sign = new Sign
                {
                    Id = entry.Id,
                    Fsw = entry.FswNotation,
                    Gloss = entry.Gloss,
                    DictionaryId = dictionaryId,
                    SgmlText = entry.Text,
                    Created = entry.Created,
                    Updated = entry.Modified,
                    CreatedBy = entry.User,
                    UpdatedBy = entry.User
                };

                signs.Add(sign);
            }

            return await Task.FromResult(signs);
        }        private static string DetermineLanguageFromType(string type)
        {
            return type.ToLower() switch
            {
                "sgn" => "sgn", // Generic sign language
                "ase" => "ase", // American Sign Language
                "bsl" => "bsl", // British Sign Language
                "fsl" => "fsl", // French Sign Language
                _ => "sgn" // Default to generic sign language
            };
        }

        // This method is in the API layer, not in the test code.
        private static bool IsValidFsw(string? input)
        {
            // FSW strings typically start with "AS" or "MS" followed by digits and symbols.
            // This regex matches a basic FSW sign string: starts with 'A' or 'M', then 'S', then at least 5 digits.
            if (string.IsNullOrWhiteSpace(input))
                return false;
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[AM]S\d{5}");
        }
    }
}
