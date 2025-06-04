using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SignPuddle.API.Models;
using SignPuddle.API.Services;

namespace SignPuddle.API.Tests.Integration
{
    public class SpmlImportIntegrationTests
    {
        private readonly SpmlImportService _spmlImportService;
        private readonly string _testDataPath;

        public SpmlImportIntegrationTests()
        {
            _spmlImportService = new SpmlImportService();
            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        [Fact]
        public async Task ImportWorkflow_ShouldImportAndPreserveDataCorrectly()
        {
            // Parse SPML file
            var spmlDocument = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);
            Assert.NotNull(spmlDocument);
            Assert.Equal("Dictionary US", spmlDocument.DictionaryName);
            Assert.Equal(10, spmlDocument.Entries.Count);

            // Convert to Dictionary
            var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument, "test-user-123");
            Assert.NotNull(dictionary);
            Assert.Equal("Dictionary US", dictionary.Name);
            Assert.Equal("test-user-123", dictionary.OwnerId);
            Assert.Equal(new DateTime(2008, 2, 18, 22, 37, 12, DateTimeKind.Utc), dictionary.Created);
            Assert.Equal(new DateTime(2011, 7, 18, 21, 40, 53, DateTimeKind.Utc), dictionary.Updated);

            // Convert to Signs
            var dictionaryId = 42;
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, dictionaryId);
            Assert.NotNull(signs);
            Assert.Equal(10, signs.Count);

            // Data integrity and attribution
            var valSign = signs.SingleOrDefault(s => s.CreatedBy == "Val");
            var adminSigns = signs.Where(s => s.CreatedBy == "admin").ToList();
            var ipSign = signs.SingleOrDefault(s => s.CreatedBy == "174.59.122.20");
            Assert.NotNull(valSign);
            Assert.Equal("test zero", valSign.Gloss);
            Assert.Contains(adminSigns, s => s.Gloss == "infirmity");
            Assert.Contains(adminSigns, s => s.Gloss == "DELAY");
            Assert.NotNull(ipSign);
            Assert.Equal("name", ipSign.Gloss);

            // Specific sign data and timestamps
            var testZeroSign = signs.First(s => s.Gloss == "test zero");
            Assert.Equal("AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494", testZeroSign.Fsw);
            Assert.Equal("we are testing SignPuddle 1.6", testZeroSign.SgmlText);
            Assert.Equal(new DateTime(2011, 7, 20, 17, 39, 2, DateTimeKind.Utc), testZeroSign.Created);
            Assert.Equal(new DateTime(2011, 7, 20, 17, 42, 9, DateTimeKind.Utc), testZeroSign.Updated);

            // All signs: dictionary ID, FSW, timestamps
            foreach (var sign in signs)
            {
                Assert.Equal(dictionaryId, sign.DictionaryId);
                Assert.False(string.IsNullOrEmpty(sign.Fsw));
                Assert.True(sign.Created > DateTime.MinValue);
                Assert.True(sign.Updated >= sign.Created);
            }

            // Content statistics
            Assert.Equal(10, signs.Count(s => !string.IsNullOrEmpty(s.Gloss)));
            Assert.Equal(2, signs.Count(s => !string.IsNullOrEmpty(s.SgmlText)));
            Assert.Equal(1, spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Video)));
            Assert.Equal(6, spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Source)));

            // Mixed content: video, long FSW, complex source
            var entryWithVideo = spmlDocument.Entries.FirstOrDefault(e => !string.IsNullOrEmpty(e.Video));
            Assert.NotNull(entryWithVideo);
            Assert.Contains("youtube.com", entryWithVideo.Video);
            Assert.Contains("iframe", entryWithVideo.Video);

            var entryWithLongFsw = spmlDocument.Entries.FirstOrDefault(e => e.Fsw?.Length > 100);
            Assert.NotNull(entryWithLongFsw);

            var entryWithComplexSource = spmlDocument.Entries.FirstOrDefault(e => e.Source?.Contains(",") == true);
            Assert.NotNull(entryWithComplexSource);
            Assert.Contains("Stuart Thiessen, Des Moines, IA", entryWithComplexSource.Source);
        }
    }
}
