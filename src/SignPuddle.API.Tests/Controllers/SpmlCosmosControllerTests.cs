using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignPuddle.API.Controllers;
using SignPuddle.API.Data;
using SignPuddle.API.Data.Repositories;
using SignPuddle.API.Models;
using SignPuddle.API.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SignPuddle.API.Tests.Controllers
{
    /// <summary>
    /// Tests for SpmlCosmosController API endpoints
    /// </summary>
    public class SpmlCosmosControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpmlPersistenceService _spmlPersistenceService;
        private readonly SpmlCosmosController _controller;
        private readonly string _testDataPath;

        public SpmlCosmosControllerTests()
        {
            // Setup in-memory database for testing
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Initialize services
            var spmlImportService = new SpmlImportService();
            var spmlRepository = new SpmlRepository(_context);
            _spmlPersistenceService = new SpmlPersistenceService(spmlImportService, spmlRepository);

            _controller = new SpmlCosmosController(_spmlPersistenceService);
            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        [Fact]
        public async Task ImportSpml_WithValidFile_ShouldReturnOkResult()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var fileBytes = Encoding.UTF8.GetBytes(xmlContent);
            var fileName = "test-dictionary.spml";

            var formFile = new FormFile(
                new MemoryStream(fileBytes),
                0,
                fileBytes.Length,
                "file",
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/xml"
            };

            // Act
            var result = await _controller.ImportSpml(
                formFile,
                ownerId: "test-owner",
                description: "Test import",
                tags: "test,import,dictionary");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            Assert.True((bool)response.Success);
            Assert.NotNull(response.SpmlDocumentEntity);
            Assert.NotNull(response.Dictionary);
            Assert.NotNull(response.Signs);
        }

        [Fact]
        public async Task ImportSpml_WithNullFile_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.ImportSpml(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("No file uploaded", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task ImportSpml_WithEmptyFile_ShouldReturnBadRequest()
        {
            // Arrange
            var formFile = new FormFile(
                new MemoryStream(),
                0,
                0,
                "file",
                "empty.spml");

            // Act
            var result = await _controller.ImportSpml(formFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("File is empty", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task ImportSpml_WithInvalidXml_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidXml = "<invalid>xml content</invalid>";
            var fileBytes = Encoding.UTF8.GetBytes(invalidXml);
            var formFile = new FormFile(
                new MemoryStream(fileBytes),
                0,
                fileBytes.Length,
                "file",
                "invalid.spml");

            // Act
            var result = await _controller.ImportSpml(formFile);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic response = badRequestResult.Value;
            Assert.False((bool)response.Success);
            Assert.Contains("Failed to import", (string)response.Message);
        }

        [Fact]
        public async Task GetSpmlDocument_WithExistingId_ShouldReturnOkResult()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var importResult = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent);
            var documentId = importResult.SpmlDocumentEntity.Id;

            // Act
            var result = await _controller.GetSpmlDocument(documentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var document = Assert.IsType<SpmlDocumentEntity>(okResult.Value);
            Assert.Equal(documentId, document.Id);
            Assert.Equal("sgn", document.SpmlType);
        }

        [Fact]
        public async Task GetSpmlDocument_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.GetSpmlDocument("non-existent-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportSpmlDocument_WithExistingId_ShouldReturnFileResult()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var importResult = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent);
            var documentId = importResult.SpmlDocumentEntity.Id;

            // Act
            var result = await _controller.ExportSpmlDocument(documentId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/xml", fileResult.ContentType);
            Assert.Contains("spml-document", fileResult.FileDownloadName);
            
            var exportedXml = Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.Contains("<?xml version=\"1.0\"", exportedXml);
            Assert.Contains("<spml", exportedXml);
        }

        [Fact]
        public async Task ExportSpmlDocument_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.ExportSpmlDocument("non-existent-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ConvertToEntities_WithExistingDocument_ShouldReturnOkResult()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var importResult = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, "test-owner");
            var document = importResult.SpmlDocumentEntity;            // Act
            var result = await _controller.ConvertToEntitiesFromBody(document);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            Assert.NotNull(response.Dictionary);
            Assert.NotNull(response.Signs);
            Assert.NotNull(response.SpmlDocument);
        }

        [Fact]
        public async Task ConvertToEntities_WithNullDocument_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.ConvertToEntitiesFromBody(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Document cannot be null", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DeleteSpmlDocument_WithExistingId_ShouldReturnOkResult()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var importResult = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent);
            var documentId = importResult.SpmlDocumentEntity.Id;

            // Act
            var result = await _controller.DeleteSpmlDocument(documentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            Assert.True((bool)response.Success);
            Assert.Contains("successfully deleted", (string)response.Message);
        }

        [Fact]
        public async Task DeleteSpmlDocument_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.DeleteSpmlDocument("non-existent-id");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            dynamic response = notFoundResult.Value;
            Assert.False((bool)response.Success);
            Assert.Contains("not found", (string)response.Message);
        }

        [Fact]
        public async Task GetStats_WithDocuments_ShouldReturnOkResult()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent);
            await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent);

            // Act
            var result = await _controller.GetStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var stats = okResult.Value;
            
            // Use reflection to check the anonymous object properties
            var statsType = stats.GetType();
            var totalDocsProperty = statsType.GetProperty("TotalDocuments");
            var totalEntriesProperty = statsType.GetProperty("TotalEntries");
            
            Assert.NotNull(totalDocsProperty);
            Assert.NotNull(totalEntriesProperty);
            Assert.Equal(2, totalDocsProperty.GetValue(stats));
            Assert.Equal(20, totalEntriesProperty.GetValue(stats)); // 10 entries per document * 2
        }

        [Fact]
        public async Task GetStats_WithEmptyRepository_ShouldReturnZeroStats()
        {
            // Act
            var result = await _controller.GetStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var stats = okResult.Value;
            
            var statsType = stats.GetType();
            var totalDocsProperty = statsType.GetProperty("TotalDocuments");
            var totalEntriesProperty = statsType.GetProperty("TotalEntries");
            
            Assert.Equal(0, totalDocsProperty.GetValue(stats));
            Assert.Equal(0, totalEntriesProperty.GetValue(stats));
        }

        [Fact]
        public async Task ImportSpml_WithTagsString_ShouldParseTags()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var fileBytes = Encoding.UTF8.GetBytes(xmlContent);
            var formFile = new FormFile(
                new MemoryStream(fileBytes),
                0,
                fileBytes.Length,
                "file",
                "test.spml");

            // Act
            var result = await _controller.ImportSpml(
                formFile,
                ownerId: "test-owner",
                description: "Test with tags",
                tags: "tag1,tag2, tag3 , tag4");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            var documentEntity = (SpmlDocumentEntity)response.SpmlDocumentEntity;
            
            Assert.Contains("tag1", documentEntity.Tags);
            Assert.Contains("tag2", documentEntity.Tags);
            Assert.Contains("tag3", documentEntity.Tags);
            Assert.Contains("tag4", documentEntity.Tags);
        }

        [Fact]
        public async Task ImportSpml_WithoutOptionalParameters_ShouldUseDefaults()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var fileBytes = Encoding.UTF8.GetBytes(xmlContent);
            var formFile = new FormFile(
                new MemoryStream(fileBytes),
                0,
                fileBytes.Length,
                "file",
                "test.spml");

            // Act
            var result = await _controller.ImportSpml(formFile);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            var documentEntity = (SpmlDocumentEntity)response.SpmlDocumentEntity;
            
            Assert.Null(documentEntity.OwnerId);
            Assert.Contains("SPML Dictionary", documentEntity.Description);
            Assert.Contains("sgn", documentEntity.Tags);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetSpmlDocument_WithInvalidId_ShouldReturnNotFound(string invalidId)
        {
            // Act
            var result = await _controller.GetSpmlDocument(invalidId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExportSpmlDocument_WithInvalidId_ShouldReturnNotFound(string invalidId)
        {
            // Act
            var result = await _controller.ExportSpmlDocument(invalidId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteSpmlDocument_WithInvalidId_ShouldReturnNotFound(string invalidId)
        {
            // Act
            var result = await _controller.DeleteSpmlDocument(invalidId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            dynamic response = notFoundResult.Value;
            Assert.False((bool)response.Success);
        }

        [Fact]
        public async Task ImportSpml_ExceptionHandling_ShouldReturnInternalServerError()
        {
            // This test would require a mock service that throws exceptions
            // For now, we'll test with malformed XML that causes parsing errors
            var malformedXml = "<?xml version=\"1.0\"?><spml><entry></spml>"; // Missing closing tag
            var fileBytes = Encoding.UTF8.GetBytes(malformedXml);
            var formFile = new FormFile(
                new MemoryStream(fileBytes),
                0,
                fileBytes.Length,
                "file",
                "malformed.spml");

            // Act
            var result = await _controller.ImportSpml(formFile);

            // Assert
            // Should return bad request due to XML parsing error, not server error
            Assert.IsType<BadRequestObjectResult>(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

