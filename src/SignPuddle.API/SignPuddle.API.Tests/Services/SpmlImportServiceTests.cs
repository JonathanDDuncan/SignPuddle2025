using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SignPuddle.API.Models;
using SignPuddle.API.Services;

namespace SignPuddle.API.Tests.Services
{
    public class SpmlImportServiceTests
    {
        private readonly SpmlImportService _spmlImportService;
        private readonly string _testDataPath;

        public SpmlImportServiceTests()
        {
            _spmlImportService = new SpmlImportService();
            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        [Fact]
        public async Task ParseSpmlFromFileAsync_WithValidFile_ShouldReturnSpmlDocument()
        {
            // Act
            var result = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("sgn", result.Type);
            Assert.Equal(4, result.PuddleId);
            Assert.Equal("Dictionary US", result.DictionaryName);
            Assert.True(result.Entries.Count > 0);
        }

        [Fact]
        public async Task ParseSpmlFromFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = "nonexistent.spml";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => _spmlImportService.ParseSpmlFromFileAsync(nonExistentPath));
        }

        [Fact]
        public async Task ParseSpmlAsync_WithValidXml_ShouldParseCorrectly()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);

            // Act
            var result = await _spmlImportService.ParseSpmlAsync(xmlContent);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("http://www.signbank.org/signpuddle1.6", result.Root);
            Assert.Equal("sgn", result.Type);
            Assert.Equal(4, result.PuddleId);
            Assert.Equal(1203374232, result.CreatedTimestamp);
            Assert.Equal(1311025253, result.ModifiedTimestamp);
            Assert.Equal(11237, result.NextId);
            Assert.Equal("Dictionary US", result.DictionaryName);
            Assert.NotNull(result.PngData);
        }

        [Fact]
        public async Task ParseSpmlAsync_ShouldParseAllEntries()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);

            // Act
            var result = await _spmlImportService.ParseSpmlAsync(xmlContent);            // Assert
            Assert.Equal(10, result.Entries.Count);

            // Test first entry
            var firstEntry = result.Entries[0];
            Assert.Equal(1, firstEntry.EntryId);
            Assert.Equal("Val", firstEntry.User);
            Assert.Equal(1311183542, firstEntry.CreatedTimestamp);
            Assert.Equal(1311183729, firstEntry.ModifiedTimestamp);
            Assert.Equal(2, firstEntry.Terms.Count);
            Assert.Equal("AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494", firstEntry.Terms[0]);
            Assert.Equal("test zero", firstEntry.Terms[1]);
            Assert.Equal("we are testing SignPuddle 1.6", firstEntry.Text);
            Assert.Contains("youtube.com", firstEntry.Video);
            Assert.Equal("Val ;-)", firstEntry.Source);
        }

        [Fact]
        public async Task ParseSpmlAsync_ShouldExtractFswAndGlossCorrectly()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);

            // Act
            var result = await _spmlImportService.ParseSpmlAsync(xmlContent);

            // Assert
            var testEntry = result.Entries[0];
            Assert.Equal("AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494", testEntry.Fsw);
            Assert.Equal("test zero", testEntry.Gloss[0]);

            var delayEntry = result.Entries[2];
            Assert.Equal("AS1ce40S1ce48S2b800M523x537S1ce40501x507S1ce48478x507S2b800498x462", delayEntry.Fsw);
            Assert.Equal("DELAY", delayEntry.Gloss[0]);
        }

        [Fact]
        public async Task ConvertToDictionaryAsync_ShouldCreateValidDictionary()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var ownerId = "test-user-123";

            // Act
            var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument, ownerId);

            // Assert
            Assert.NotNull(dictionary);
            Assert.Equal("Dictionary US", dictionary.Name);
            Assert.Equal("Imported from SPML puddle 4", dictionary.Description);
            Assert.True(dictionary.IsPublic);
            Assert.Equal(ownerId, dictionary.OwnerId);
            Assert.Equal(spmlDocument.Created, dictionary.Created);
            Assert.Equal(spmlDocument.Modified, dictionary.Updated);
        }

        [Fact]
        public async Task ConvertToDictionaryAsync_WithNullOwner_ShouldCreateDictionaryWithoutOwner()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            // Act
            var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument);

            // Assert
            Assert.NotNull(dictionary);
            Assert.Null(dictionary.OwnerId);
        }

        [Fact]
        public async Task ConvertToSignsAsync_ShouldCreateValidSigns()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var dictionaryId = "123";

            // Act
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, dictionaryId);

            // Assert            Assert.NotNull(signs);
            Assert.Equal(10, signs.Count);

            // Test first sign
            var firstSign = signs[0];
            Assert.Equal(1, firstSign.PuddleSignId);
            Assert.Equal("AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494", firstSign.Fsw);
            Assert.Equal("test zero", firstSign.Gloss[0]);
            Assert.Equal(dictionaryId, firstSign.DictionaryId);
            Assert.Equal("we are testing SignPuddle 1.6", firstSign.Description);
            Assert.Equal("Val", firstSign.CreatedBy);
            Assert.Equal("Val", firstSign.UpdatedBy);

            // Test entry without gloss
            var infirmitySign = signs.FirstOrDefault(s => s.Gloss != null && s.Gloss.Contains("infirmity"));
            Assert.NotNull(infirmitySign);
            Assert.Equal("AS1c500S1c509S31300S20500M541x560S31300482x482S1c500511x477S1c509475x523S20500498x549", infirmitySign.Fsw);
            Assert.Equal("admin", infirmitySign.CreatedBy);
        }

        [Fact]
        public async Task ConvertToSignsAsync_ShouldSkipEntriesWithoutFsw()
        {
            // Arrange - Create a minimal SPML with one entry without FSW
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE spml SYSTEM ""http://www.signpuddle.net/spml_1.6.dtd"">
<spml root=""http://www.signbank.org/signpuddle1.6"" type=""sgn"" puddle=""4"" cdt=""1203374232"" mdt=""1311025253"" nextid=""11237"">
  <term><![CDATA[Test Dictionary]]></term>
  <entry id=""1"" cdt=""1311183542"" mdt=""1311183729"" usr=""test"">
    <term><![CDATA[no fsw here]]></term>
  </entry>
  <entry id=""2"" cdt=""1311183542"" mdt=""1311183729"" usr=""test"">
    <term>AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494</term>
    <term><![CDATA[valid sign]]></term>
  </entry>
</spml>";

            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var dictionaryId = "123";

            // Act
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, dictionaryId);

            // Assert
            Assert.Single(signs); // Only one sign should be created (the one with FSW)
            Assert.Equal("valid sign", signs[0].Gloss[0]);
        }

        [Fact]
        public async Task ParseSpmlAsync_WithComplexEntry_ShouldParseAllFields()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
 
            // Act
            var result = await _spmlImportService.ParseSpmlAsync(xmlContent);
 
            // Assert
            var delayEntry = result.Entries[2]; // Entry with id="3" (DELAY)
            Assert.Equal(3, delayEntry.EntryId);
            Assert.Equal("admin", delayEntry.User);
            Assert.Equal("DELAY", delayEntry.Gloss[0]);
            Assert.Equal("Delay, postpone, move forward in time", delayEntry.Text);
            Assert.Equal("Stuart Thiessen, Des Moines, IA", delayEntry.Source);
            Assert.Null(delayEntry.Video); // This entry doesn't have video
        }

        [Fact]
        public void SpmlDocument_TimestampConversion_ShouldWorkCorrectly()
        {
            // Arrange
            var spmlDocument = new SpmlDocument
            {
                CreatedTimestamp = 1203374232, // Should convert to 2008-02-18
                ModifiedTimestamp = 1311025253  // Should convert to 2011-07-18
            };

            // Act
            var createdDate = spmlDocument.Created;
            var modifiedDate = spmlDocument.Modified;

            // Assert
            Assert.Equal(2008, createdDate.Year);
            Assert.Equal(2, createdDate.Month);
            Assert.Equal(18, createdDate.Day);

            Assert.Equal(2011, modifiedDate.Year);
            Assert.Equal(7, modifiedDate.Month);
            Assert.Equal(18, modifiedDate.Day);
        }

        [Fact]
        public void SpmlEntry_TimestampConversion_ShouldWorkCorrectly()
        {
            // Arrange
            var entry = new SpmlEntry
            {
                CreatedTimestamp = 1311183542, // Should convert to 2011-07-20
                ModifiedTimestamp = 1311183729  // Should convert to 2011-07-20
            };

            // Act
            var createdDate = entry.Created;
            var modifiedDate = entry.Modified;

            // Assert
            Assert.Equal(2011, createdDate.Year);
            Assert.Equal(7, createdDate.Month);
            Assert.Equal(20, createdDate.Day);

            Assert.Equal(2011, modifiedDate.Year);
            Assert.Equal(7, modifiedDate.Month);
            Assert.Equal(20, modifiedDate.Day);
        }
    }
}
