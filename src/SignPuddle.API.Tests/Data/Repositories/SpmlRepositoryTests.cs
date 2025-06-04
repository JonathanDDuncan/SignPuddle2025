using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignPuddle.API.Data;
using SignPuddle.API.Data.Repositories;
using SignPuddle.API.Models;
using SignPuddle.API.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SignPuddle.API.Tests.Data.Repositories
{
    /// <summary>
    /// Tests for SpmlRepository CRUD operations
    /// </summary>
    public class SpmlRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpmlRepository _repository;
        private readonly ISpmlImportService _spmlImportService;
        private readonly string _testDataPath;

        public SpmlRepositoryTests()
        {
            // Setup in-memory database for testing
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            _repository = new SpmlRepository(_context);
            _spmlImportService = new SpmlImportService();
            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        [Fact]
        public async Task SaveAsync_WithValidEntity_ShouldSaveSuccessfully()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var entity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "test-owner");

            // Act
            var savedEntity = await _repository.SaveAsync(entity);

            // Assert
            Assert.NotNull(savedEntity);
            Assert.NotNull(savedEntity.Id);
            Assert.Equal("sgn", savedEntity.PartitionKey);
            Assert.Equal("test-owner", savedEntity.OwnerId);
            Assert.Equal(10, savedEntity.EntryCount);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnEntity()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var entity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "test-owner");
            var savedEntity = await _repository.SaveAsync(entity);

            // Act
            var retrievedEntity = await _repository.GetSpmlDocumentByIdAsync(savedEntity.Id);

            // Assert
            Assert.NotNull(retrievedEntity);
            Assert.Equal(savedEntity.Id, retrievedEntity.Id);
            Assert.Equal(savedEntity.PartitionKey, retrievedEntity.PartitionKey);
            Assert.Equal(savedEntity.OwnerId, retrievedEntity.OwnerId);
            Assert.Equal(savedEntity.EntryCount, retrievedEntity.EntryCount);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetSpmlDocumentByIdAsync("non-existent-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleEntities_ShouldReturnAll()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            var entity1 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "owner1");
            var entity2 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "owner2");
            var entity3 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "owner3");

            await _repository.SaveAsync(entity1);
            await _repository.SaveAsync(entity2);
            await _repository.SaveAsync(entity3);

            // Act
            var allEntities = await _repository.GetAllSpmlDocumentsAsync();

            // Assert
            Assert.Equal(3, allEntities.Count());
            Assert.All(allEntities, entity => Assert.Equal("sgn", entity.PartitionKey));
        }

        [Fact]
        public async Task GetByTypeAsync_ShouldReturnOnlyMatchingType()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            // Create entities with different types
            var sgnEntity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "owner1");
            
            // Create a modified entity with different type for testing
            var modifiedDocument = new SpmlDocument
            {
                Type = "test",
                PuddleId = 5,
                Terms = new List<string> { "Test Dictionary" },
                Entries = new List<SpmlEntry>()
            };
            var testEntity = SpmlDocumentEntity.FromSpmlDocument(modifiedDocument, xmlContent, "owner2");

            await _repository.SaveAsync(sgnEntity);
            await _repository.SaveAsync(testEntity);

            // Act
            var sgnEntities = await _repository.GetSpmlDocumentsByTypeAsync("sgn");
            var testEntities = await _repository.GetSpmlDocumentsByTypeAsync("test");

            // Assert
            Assert.Single(sgnEntities);
            Assert.Single(testEntities);
            Assert.Equal("sgn", sgnEntities.First().SpmlType);
            Assert.Equal("test", testEntities.First().SpmlType);
        }

        [Fact]
        public async Task GetByPuddleIdAsync_ShouldReturnOnlyMatchingPuddleId()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            var entity1 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "owner1");
            
            // Create entity with different puddle ID
            var modifiedDocument = new SpmlDocument
            {
                Type = "sgn",
                PuddleId = 999,
                Terms = new List<string> { "Different Puddle" },
                Entries = new List<SpmlEntry>()
            };
            var entity2 = SpmlDocumentEntity.FromSpmlDocument(modifiedDocument, xmlContent, "owner2");

            await _repository.SaveAsync(entity1);
            await _repository.SaveAsync(entity2);

            // Act
            var puddle4Entities = await _repository.GetSpmlDocumentsByPuddleIdAsync(4);
            var puddle999Entities = await _repository.GetSpmlDocumentsByPuddleIdAsync(999);

            // Assert
            Assert.Single(puddle4Entities);
            Assert.Single(puddle999Entities);
            Assert.Equal(4, puddle4Entities.First().PuddleId);
            Assert.Equal(999, puddle999Entities.First().PuddleId);
        }

        [Fact]
        public async Task GetByOwnerAsync_ShouldReturnOnlyOwnerEntities()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            var targetOwner = "target-owner";
            var otherOwner = "other-owner";

            var entity1 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, targetOwner);
            var entity2 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, targetOwner);
            var entity3 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, otherOwner);

            await _repository.SaveAsync(entity1);
            await _repository.SaveAsync(entity2);
            await _repository.SaveAsync(entity3);

            // Act
            var targetOwnerEntities = await _repository.GetSpmlDocumentsByOwnerAsync(targetOwner);
            var otherOwnerEntities = await _repository.GetSpmlDocumentsByOwnerAsync(otherOwner);

            // Assert
            Assert.Equal(2, targetOwnerEntities.Count());
            Assert.Single(otherOwnerEntities);
            Assert.All(targetOwnerEntities, entity => Assert.Equal(targetOwner, entity.OwnerId));
            Assert.All(otherOwnerEntities, entity => Assert.Equal(otherOwner, entity.OwnerId));
        }

        [Fact]
        public async Task UpdateAsync_WithModifiedEntity_ShouldUpdateSuccessfully()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var entity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "test-owner");
            var savedEntity = await _repository.SaveAsync(entity);

            // Modify the entity
            savedEntity.Description = "Updated description";
            savedEntity.Tags.Add("updated");

            // Act
            var updatedEntity = await _repository.UpdateSpmlDocumentAsync(savedEntity);

            // Assert
            Assert.NotNull(updatedEntity);
            Assert.Equal("Updated description", updatedEntity.Description);
            Assert.Contains("updated", updatedEntity.Tags);

            // Verify the update persisted
            var retrievedEntity = await _repository.GetSpmlDocumentByIdAsync(savedEntity.Id);
            Assert.Equal("Updated description", retrievedEntity.Description);
            Assert.Contains("updated", retrievedEntity.Tags);
        }

        [Fact]
        public async Task DeleteAsync_WithExistingEntity_ShouldReturnTrue()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var entity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "test-owner");
            var savedEntity = await _repository.SaveAsync(entity);

            // Act
            var deleteResult = await _repository.DeleteSpmlDocumentAsync(savedEntity.Id);

            // Assert
            Assert.True(deleteResult);

            // Verify entity is deleted
            var retrievedEntity = await _repository.GetSpmlDocumentByIdAsync(savedEntity.Id);
            Assert.Null(retrievedEntity);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Act
            var deleteResult = await _repository.DeleteSpmlDocumentAsync("non-existent-id");

            // Assert
            Assert.False(deleteResult);
        }

        [Fact]
        public async Task ExportAsXmlAsync_WithValidEntity_ShouldReturnFormattedXml()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);
            var entity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "test-owner");
            var savedEntity = await _repository.SaveAsync(entity);

            // Act
            var exportedXml = await _repository.ExportSpmlDocumentAsXmlAsync(savedEntity.Id);

            // Assert
            Assert.NotNull(exportedXml);
            Assert.Contains("<?xml version=\"1.0\"", exportedXml);
            Assert.Contains("<!DOCTYPE spml", exportedXml);
            Assert.Contains("<spml", exportedXml);
            Assert.Contains("type=\"sgn\"", exportedXml);
            Assert.Contains("puddle=\"4\"", exportedXml);
            Assert.Contains("Dictionary US", exportedXml);
        }

        [Fact]
        public async Task ExportAsXmlAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.ExportSpmlDocumentAsXmlAsync("non-existent-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStatsAsync_WithMultipleEntities_ShouldReturnCorrectStats()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

            // Create multiple entities
            var entity1 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "owner1");
            var entity2 = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, xmlContent, "owner2");
            
            // Create entity with different type
            var modifiedDocument = new SpmlDocument
            {
                Type = "test",
                PuddleId = 5,
                Terms = new List<string> { "Test Dictionary" },
                Entries = new List<SpmlEntry>
                {
                    new SpmlEntry { Id = 1, Terms = new List<string> { "test" } }
                }
            };
            var entity3 = SpmlDocumentEntity.FromSpmlDocument(modifiedDocument, xmlContent, "owner3");

            await _repository.SaveAsync(entity1);
            await _repository.SaveAsync(entity2);
            await _repository.SaveAsync(entity3);

            // Act
            var stats = await _repository.GetSpmlDocumentStatsAsync();

            // Assert
            Assert.Equal(3, stats.TotalDocuments);
            Assert.Equal(21, stats.TotalEntries); // 10 + 10 + 1
            Assert.Equal(2, stats.DocumentsByType.Count);
            Assert.Equal(2, stats.DocumentsByType["sgn"]);
            Assert.Equal(1, stats.DocumentsByType["test"]);
        }

        [Fact]
        public async Task GetStatsAsync_WithEmptyRepository_ShouldReturnZeroStats()
        {
            // Act
            var stats = await _repository.GetSpmlDocumentStatsAsync();

            // Assert
            Assert.Equal(0, stats.TotalDocuments);
            Assert.Equal(0, stats.TotalEntries);
            Assert.Empty(stats.DocumentsByType);
        }

        [Fact]
        public async Task SaveAsync_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _repository.SaveAsync(null));
        }

        [Fact]
        public async Task UpdateAsync_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _repository.UpdateSpmlDocumentAsync(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull(string invalidId)
        {
            // Act
            var result = await _repository.GetSpmlDocumentByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse(string invalidId)
        {
            // Act
            var result = await _repository.DeleteSpmlDocumentAsync(invalidId);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExportAsXmlAsync_WithInvalidId_ShouldReturnNull(string invalidId)
        {
            // Act
            var result = await _repository.ExportSpmlDocumentAsXmlAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

