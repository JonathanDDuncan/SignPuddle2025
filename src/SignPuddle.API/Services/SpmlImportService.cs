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
                IsPublic = true,
                OwnerId = ownerId,
                Created = spmlDocument.Created,
                Updated = spmlDocument.Modified
            };

            return await Task.FromResult(dictionary);
        }
        public async Task<List<Sign>> ConvertToSignsAsync(SpmlDocument spmlDocument, int dictionaryId)
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
                    Fsw = entry.FswNotation ?? string.Empty,
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
        }        /// <summary>
                 /// Determines if the input string is a valid Formal SignWriting (FSW) string.
                 /// </summary>
        private static bool IsValidFsw(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // FSW sign: starts with 'A' or 'M', then 'S', then 3 hex digits, then 2 hex digits, then a BLMR character, then 3 digits, 'x', 3 digits
            // Example: AS10000B500x500
            // fswSignPattern: Formal SignWriting (FSW) sign string: an optional sort group, followed by a required box position (B, L, M, or R for Box, Left, Middle, Right), a coordinate, and zero or more positioned symbols.
            var fswSignPattern = @"^(A(S[123][0-9a-f]{2}[0-5][0-9a-f])+)?[BLMR]([0-9]{3}x[0-9]{3})(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*$";

            // FSW spatial: S + 3 hex + 2 hex + 3 digits x 3 digits
            var fswSpatialPattern = @"^S[0-9a-fA-F]{3}[0-5][0-9a-fA-F][0-9]{3}x[0-9]{3}";
            // FSW symbol: S + 3 hex + 2 hex
            var fswSymbolPattern = @"^S[0-9a-fA-F]{3}[0-5][0-9a-fA-F]";
            // FSW coordinate: 3 digits x 3 digits
            var fswCoordPattern = @"^[0-9]{3}x[0-9]{3}";

            return System.Text.RegularExpressions.Regex.IsMatch(input, fswSignPattern)
                || System.Text.RegularExpressions.Regex.IsMatch(input, fswSpatialPattern)
                || System.Text.RegularExpressions.Regex.IsMatch(input, fswSymbolPattern)
                || System.Text.RegularExpressions.Regex.IsMatch(input, fswCoordPattern);
        }
    }
}
