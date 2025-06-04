using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignPuddle.API.Data;
using SignPuddle.API.Data.Repositories;
using SignPuddle.API.Models;
using SignPuddle.API.Services;
using SignPuddle.API.Tests.Helpers; // Added
using SignPuddle.API; // Added for Program
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SignPuddle.API.Tests.Services
{
    /// <summary>
    /// Tests for SPML CosmosDB persistence functionality
    /// </summary>
    public class SpmlPersistenceServiceTests : IDisposable
    {
        private readonly ApiTestsWebApplicationFactory<Program> _factory; // Changed
        private readonly IServiceScope _scope; // Added
        private readonly ApplicationDbContext _context;
        private readonly ISpmlImportService _spmlImportService;
        private readonly ISpmlRepository _spmlRepository;
        private readonly ISpmlPersistenceService _spmlPersistenceService;
        private readonly string _testDataPath;

        public SpmlPersistenceServiceTests()
        {
            _factory = new ApiTestsWebApplicationFactory<Program>();
            _scope = _factory.Services.CreateScope();

            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _spmlImportService = _scope.ServiceProvider.GetRequiredService<ISpmlImportService>();
            _spmlRepository = _scope.ServiceProvider.GetRequiredService<ISpmlRepository>(); 
            _spmlPersistenceService = _scope.ServiceProvider.GetRequiredService<ISpmlPersistenceService>();

            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _factory?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task SaveSpmlDocumentAsync_WithValidDocument_ShouldSaveSuccessfully()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var ownerId = "test-user-123";
            var description = "Test SPML document";
            var tags = new List<string> { "test", "sgn", "dictionary" };

            // Act
            var result = await _spmlPersistenceService.SaveSpmlDocumentAsync(
                spmlDocument, 
                xmlContent, 
                ownerId, 
                description, 
                tags);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Assert.Equal(ownerId, result.OwnerId);
            Assert.Equal(description, result.Description);
            Assert.Equal(3, result.Tags.Count);
            Assert.Contains("test", result.Tags);
            Assert.Contains("sgn", result.Tags);
            Assert.Contains("dictionary", result.Tags);
            Assert.Equal("sgn", result.SpmlType);
            Assert.Equal(4, result.PuddleId);
            Assert.Equal(10, result.EntryCount);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithValidXml_ShouldReturnSuccessResult()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var ownerId = "test-user-456";
            var description = "Imported test dictionary";
            var tags = new List<string> { "import", "test" };

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(
                xmlContent, 
                ownerId, 
                description, 
                tags);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.NotNull(result.Dictionary);
            Assert.NotNull(result.Signs);

            // Assert SpmlDocumentEntity properties
            Assert.NotNull(result.SpmlDocumentEntity.Id);
            Assert.Equal(ownerId, result.SpmlDocumentEntity.OwnerId);
            Assert.Equal(description, result.SpmlDocumentEntity.Description);
            
            // Tags should include those passed in and potentially auto-generated ones like type
            // Assuming "sgn" is added from the document type, and "import", "test" are from input.
            Assert.Equal(3, result.SpmlDocumentEntity.Tags.Count); 
            Assert.Contains("import", result.SpmlDocumentEntity.Tags);
            Assert.Contains("test", result.SpmlDocumentEntity.Tags);
            Assert.Contains("sgn", result.SpmlDocumentEntity.Tags); 
            
            Assert.Equal("sgn", result.SpmlDocumentEntity.SpmlType); // Based on sgn4-small.spml
            Assert.Equal(4, result.SpmlDocumentEntity.PuddleId);    // Based on sgn4-small.spml
            Assert.Equal(10, result.SpmlDocumentEntity.EntryCount); // Based on sgn4-small.spml

            // Assert other SpmlImportResult properties
            Assert.Contains("Successfully imported SPML document", result.Message);
            Assert.Equal(7, result.Signs.Count); // Should have 7 valid signs with FSW notation from sgn4-small.spml
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithInvalidXml_ShouldReturnFailureResult()
        {
            // Arrange
            var invalidXml = "<invalid>xml content</invalid>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(invalidXml);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.Equal("Failed to parse SPML document: Test exception", result.Message);
            Assert.Equal("Test exception", result.ErrorMessage); // Changed from Error to ErrorMessage
        }

        [Fact]
        public async Task GetSpmlDocumentAsync_WithExistingId_ShouldReturnDocument()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var savedEntity = await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent);

            // Act
            var retrievedEntity = await _spmlPersistenceService.GetSpmlDocumentAsync(savedEntity.Id);

            // Assert
            Assert.NotNull(retrievedEntity);
            Assert.Equal(savedEntity.Id, retrievedEntity.Id);
            Assert.Equal(savedEntity.SpmlType, retrievedEntity.SpmlType);
            Assert.Equal(savedEntity.PuddleId, retrievedEntity.PuddleId);
            Assert.Equal(savedEntity.EntryCount, retrievedEntity.EntryCount);
        }

        [Fact]
        public async Task GetSpmlDocumentAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = await _spmlPersistenceService.GetSpmlDocumentAsync("non-existent-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ExportSpmlDocumentAsXmlAsync_WithExistingDocument_ShouldReturnXml()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var savedEntity = await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent);

            // Act
            var exportedXml = await _spmlPersistenceService.ExportSpmlDocumentAsXmlAsync(savedEntity.Id);

            // Assert
            Assert.NotNull(exportedXml);
            Assert.Contains("<?xml version=\"1.0\"", exportedXml);
            Assert.Contains("<spml", exportedXml);
            Assert.Contains("type=\"sgn\"", exportedXml);
            Assert.Contains("puddle=\"4\"", exportedXml);
        }

        [Fact]
        public async Task ConvertSpmlDocumentToEntitiesAsync_ShouldReturnValidConversion()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var savedEntity = await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent, "test-owner");

            // Act
            var conversionResult = await _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(savedEntity);

            // Assert
            Assert.NotNull(conversionResult);
            Assert.NotNull(conversionResult.Dictionary);
            Assert.NotNull(conversionResult.Signs);
            Assert.NotNull(conversionResult.SpmlDocument);

            // Check dictionary properties
            Assert.Equal("Dictionary US", conversionResult.Dictionary.Name);
            Assert.Equal("en-US", conversionResult.Dictionary.Language);
            Assert.Equal("test-owner", conversionResult.Dictionary.OwnerId);

            // Check signs
            Assert.Equal(7, conversionResult.Signs.Count);
            var firstSign = conversionResult.Signs[0];
            Assert.Equal("AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494", firstSign.Fsw);
            Assert.Equal("test zero", firstSign.Gloss);
        }

        [Fact]
        public async Task GetAllSpmlDocumentsAsync_WithMultipleDocuments_ShouldReturnAll()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            // Save multiple documents
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent, "user1");
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent, "user2");
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent, "user3");

            // Act
            var allDocuments = await _spmlPersistenceService.GetAllSpmlDocumentsAsync();

            // Assert
            Assert.Equal(3, allDocuments.Count());
            Assert.All(allDocuments, doc => Assert.Equal("sgn", doc.SpmlType));
        }

        [Fact]
        public async Task GetSpmlDocumentsByOwnerAsync_ShouldReturnOnlyOwnerDocuments()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            var targetOwnerId = "target-owner";
            var otherOwnerId = "other-owner";

            // Save documents for different owners
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent, targetOwnerId);
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent, targetOwnerId);
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent, otherOwnerId);

            // Act
            var ownerDocuments = await _spmlPersistenceService.GetSpmlDocumentsByOwnerAsync(targetOwnerId);

            // Assert
            Assert.Equal(2, ownerDocuments.Count());
            Assert.All(ownerDocuments, doc => Assert.Equal(targetOwnerId, doc.OwnerId));
        }

        [Fact]
        public async Task DeleteSpmlDocumentAsync_WithExistingDocument_ShouldReturnTrue()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var savedEntity = await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent);

            // Act
            var deleteResult = await _spmlPersistenceService.DeleteSpmlDocumentAsync(savedEntity.Id);

            // Assert
            Assert.True(deleteResult);

            // Verify document is deleted
            var retrievedEntity = await _spmlPersistenceService.GetSpmlDocumentAsync(savedEntity.Id);
            Assert.Null(retrievedEntity);
        }

        [Fact]
        public async Task DeleteSpmlDocumentAsync_WithNonExistentDocument_ShouldReturnFalse()
        {
            // Act
            var deleteResult = await _spmlPersistenceService.DeleteSpmlDocumentAsync("non-existent-id");

            // Assert
            Assert.False(deleteResult);
        }

        [Fact]
        public async Task GetSpmlDocumentStatsAsync_WithDocuments_ShouldReturnCorrectStats()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            // Save multiple documents
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent);
            await _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, xmlContent);

            // Act
            var stats = await _spmlPersistenceService.GetSpmlDocumentStatsAsync();

            // Assert
            Assert.Equal(2, stats.TotalDocuments);
            Assert.Equal(20, stats.TotalEntries); // 10 entries per document * 2 documents
            Assert.True(stats.DocumentsByType.ContainsKey("sgn"));
            Assert.Equal(2, stats.DocumentsByType["sgn"]);
        }

        [Fact]
        public void SpmlDocumentEntity_FromSpmlDocument_ShouldCreateValidEntity()
        {
            // Arrange
            var xmlContent = "<test>content</test>";
            var spmlDocument = new SpmlDocument
            {
                Type = "sgn",
                PuddleId = 4,
                Terms = new List<string> { "Test Dictionary" },
                Entries = new List<SpmlEntry>
                {
                    new SpmlEntry { Id = 1, Terms = new List<string> { "test" } },
                    new SpmlEntry { Id = 2, Terms = new List<string> { "another" } }
                }
            };
            var ownerId = "test-owner";

            // Act
            var entity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, ownerId);

            // Assert
            Assert.NotNull(entity.Id);
            Assert.Equal("sgn", entity.PartitionKey);
            Assert.Equal("spml", entity.DocumentType);
            Assert.Equal(spmlDocument, entity.SpmlDocument);
            Assert.Equal(xmlContent, entity.OriginalXml);
            Assert.Equal(ownerId, entity.OwnerId);
            Assert.Equal("SPML Dictionary: Test Dictionary", entity.Description);
            Assert.Contains("sgn", entity.Tags);
            Assert.Equal(2, entity.EntryCount);
            Assert.Equal("sgn", entity.SpmlType);
            Assert.Equal(4, entity.PuddleId);
            Assert.Equal("Test Dictionary", entity.DictionaryName);
        }
    }
}
