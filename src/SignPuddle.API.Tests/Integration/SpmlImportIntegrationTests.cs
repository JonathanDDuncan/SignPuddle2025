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
        }        [Fact]
        public async Task FullImportWorkflow_ShouldImportSpmlToDictionaryAndSigns()
        {
            // Step 1: Parse SPML file
            var spmlDocument = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);
            Assert.NotNull(spmlDocument);
            Assert.Equal("Dictionary US", spmlDocument.DictionaryName);
            Assert.Equal(10, spmlDocument.Entries.Count);

            // Step 2: Convert to Dictionary
            var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument, "test-user-123");
            Assert.NotNull(dictionary);
            Assert.Equal("Dictionary US", dictionary.Name);
            Assert.Equal("sgn", dictionary.Language);
            Assert.Equal("test-user-123", dictionary.OwnerId);

            // Step 3: Convert to Signs (simulating dictionary ID assignment)
            var dictionaryId = 42; // Simulated ID from database
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, dictionaryId);
            Assert.NotNull(signs);
            Assert.Equal(10, signs.Count);

            // Step 4: Verify data integrity
            foreach (var sign in signs)
            {
                Assert.Equal(dictionaryId, sign.DictionaryId);
                Assert.NotNull(sign.Fsw);
                Assert.True(sign.Fsw.Length > 0);
                Assert.True(sign.Created > DateTime.MinValue);
                Assert.True(sign.Updated > DateTime.MinValue);
            }

            // Step 5: Verify specific sign data
            var testZeroSign = signs.Find(s => s.Gloss == "test zero");
            Assert.NotNull(testZeroSign);
            Assert.Equal("AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494", testZeroSign.Fsw);
            Assert.Equal("we are testing SignPuddle 1.6", testZeroSign.SgmlText);
            Assert.Equal("Val", testZeroSign.CreatedBy);

            var delaySign = signs.Find(s => s.Gloss == "DELAY");
            Assert.NotNull(delaySign);
            Assert.Equal("AS1ce40S1ce48S2b800M523x537S1ce40501x507S1ce48478x507S2b800498x462", delaySign.Fsw);
            Assert.Equal("Delay, postpone, move forward in time", delaySign.SgmlText);
            Assert.Equal("admin", delaySign.CreatedBy);
        }

        [Fact]
        public async Task ImportStatistics_ShouldProvideCorrectCounts()
        {
            // Arrange & Act
            var spmlDocument = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, 1);

            // Assert
            var entriesWithGloss = signs.Count(s => !string.IsNullOrEmpty(s.Gloss));
            var entriesWithText = signs.Count(s => !string.IsNullOrEmpty(s.SgmlText));
            var entriesWithVideo = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Video));
            var entriesWithSource = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Source));            Assert.Equal(10, signs.Count); // Total signs
            Assert.Equal(10, entriesWithGloss); // All entries have gloss
            Assert.Equal(2, entriesWithText); // Only 2 entries have text descriptions
            Assert.Equal(1, entriesWithVideo); // Only 1 entry has video
            Assert.Equal(6, entriesWithSource); // 6 entries have source attribution
        }

        [Fact]
        public async Task ImportPreservesUserAttribution_ShouldMaintainOriginalCreators()
        {
            // Arrange & Act
            var spmlDocument = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, 1);

            // Assert
            var valSigns = signs.Where(s => s.CreatedBy == "Val").ToList();
            var adminSigns = signs.Where(s => s.CreatedBy == "admin").ToList();
            var ipSigns = signs.Where(s => s.CreatedBy == "174.59.122.20").ToList();            Assert.Single(valSigns);
            Assert.Equal(8, adminSigns.Count);
            Assert.Single(ipSigns);

            // Verify that user attribution is preserved correctly
            Assert.Equal("test zero", valSigns.First().Gloss);
            Assert.Contains(adminSigns, s => s.Gloss == "infirmity");
            Assert.Contains(adminSigns, s => s.Gloss == "DELAY");
            Assert.Equal("name", ipSigns.First().Gloss);
        }

        [Fact]
        public async Task ImportHandlesMixedContentCorrectly()
        {
            // This test verifies that the import handles entries with various content types
            var spmlDocument = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);

            // Verify different entry types are handled
            var entryWithVideo = spmlDocument.Entries.First(e => !string.IsNullOrEmpty(e.Video));
            Assert.Contains("youtube.com", entryWithVideo.Video);
            Assert.Contains("iframe", entryWithVideo.Video);

            var entryWithLongFsw = spmlDocument.Entries.First(e => e.FswNotation?.Length > 100);
            Assert.NotNull(entryWithLongFsw);
            Assert.True(entryWithLongFsw.FswNotation.Length > 100);

            var entryWithComplexSource = spmlDocument.Entries.First(e => e.Source?.Contains(",") == true);
            Assert.NotNull(entryWithComplexSource);
            Assert.Contains("Stuart Thiessen, Des Moines, IA", entryWithComplexSource.Source);
        }

        [Fact]
        public async Task ImportMetadata_ShouldPreserveTimestamps()
        {
            // Arrange & Act
            var spmlDocument = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);
            var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument);
            var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, 1);            // Assert - Dictionary timestamps
            Assert.Equal(new DateTime(2008, 2, 18, 22, 37, 12, DateTimeKind.Utc), dictionary.Created);
            Assert.Equal(new DateTime(2011, 7, 18, 21, 40, 53, DateTimeKind.Utc), dictionary.Updated);

            // Assert - Sign timestamps
            var testZeroSign = signs.First(s => s.Gloss == "test zero");
            Assert.Equal(new DateTime(2011, 7, 20, 17, 39, 2, DateTimeKind.Utc), testZeroSign.Created);
            Assert.Equal(new DateTime(2011, 7, 20, 17, 42, 9, DateTimeKind.Utc), testZeroSign.Updated);

            // Verify all signs have valid timestamps
            foreach (var sign in signs)
            {
                Assert.True(sign.Created > DateTime.MinValue);
                Assert.True(sign.Updated >= sign.Created);
            }
        }
    }
}
