using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignPuddle.API.Data;
using SignPuddle.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SignPuddle.API.Tests.Integration
{
    /// <summary>
    /// Integration tests for SPML CosmosDB functionality
    /// </summary>
    public class SpmlCosmosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _testDataPath;

        public SpmlCosmosIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
                });
            });

            _client = _factory.CreateClient();
            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        [Fact]
        public async Task ImportSpml_EndToEnd_ShouldWorkCorrectly()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "test-dictionary.spml");
            content.Add(new StringContent("integration-test-owner"), "ownerId");
            content.Add(new StringContent("Integration test dictionary"), "description");
            content.Add(new StringContent("integration,test,dictionary"), "tags");

            // Act
            var response = await _client.PostAsync("/api/spmlcosmos/import", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SpmlImportToCosmosResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.NotNull(result.Dictionary);
            Assert.NotNull(result.Signs);
            Assert.Equal("integration-test-owner", result.SpmlDocumentEntity.OwnerId);
            Assert.Equal("Integration test dictionary", result.SpmlDocumentEntity.Description);
        }

        [Fact]
        public async Task GetSpmlDocument_AfterImport_ShouldReturnDocument()
        {
            // Arrange - First import a document
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "test-dictionary.spml");

            var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
            importResponse.EnsureSuccessStatusCode();
            
            var importContent = await importResponse.Content.ReadAsStringAsync();
            var importResult = JsonSerializer.Deserialize<SpmlImportToCosmosResult>(importContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var documentId = importResult.SpmlDocumentEntity.Id;

            // Act
            var getResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}");

            // Assert
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var document = JsonSerializer.Deserialize<SpmlDocumentEntity>(getContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(document);
            Assert.Equal(documentId, document.Id);
            Assert.Equal("sgn", document.SpmlType);
            Assert.Equal(4, document.PuddleId);
        }

        [Fact]
        public async Task ExportSpmlDocument_AfterImport_ShouldReturnXml()
        {
            // Arrange - First import a document
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "test-dictionary.spml");

            var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
            importResponse.EnsureSuccessStatusCode();
            
            var importContent = await importResponse.Content.ReadAsStringAsync();
            var importResult = JsonSerializer.Deserialize<SpmlImportToCosmosResult>(importContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var documentId = importResult.SpmlDocumentEntity.Id;

            // Act
            var exportResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}/export");

            // Assert
            exportResponse.EnsureSuccessStatusCode();
            Assert.Equal("application/xml", exportResponse.Content.Headers.ContentType.MediaType);
            
            var exportedXml = await exportResponse.Content.ReadAsStringAsync();
            Assert.Contains("<?xml version=\"1.0\"", exportedXml);
            Assert.Contains("<spml", exportedXml);
            Assert.Contains("type=\"sgn\"", exportedXml);
            Assert.Contains("puddle=\"4\"", exportedXml);
        }

        [Fact]
        public async Task ConvertToEntities_AfterImport_ShouldReturnConversion()
        {
            // Arrange - First import a document
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "test-dictionary.spml");
            content.Add(new StringContent("integration-test-owner"), "ownerId");

            var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
            importResponse.EnsureSuccessStatusCode();
            
            var importContent = await importResponse.Content.ReadAsStringAsync();
            var importResult = JsonSerializer.Deserialize<SpmlImportToCosmosResult>(importContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var document = importResult.SpmlDocumentEntity;

            // Act
            var convertContent = new StringContent(
                JsonSerializer.Serialize(document, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json");

            var convertResponse = await _client.PostAsync("/api/spmlcosmos/convert", convertContent);

            // Assert
            convertResponse.EnsureSuccessStatusCode();
            var convertResponseContent = await convertResponse.Content.ReadAsStringAsync();
            
            // Parse as dynamic object since we don't have a specific type for the conversion result
            var jsonDoc = JsonDocument.Parse(convertResponseContent);
            var root = jsonDoc.RootElement;
            
            Assert.True(root.TryGetProperty("dictionary", out var dictionaryElement));
            Assert.True(root.TryGetProperty("signs", out var signsElement));
            Assert.True(root.TryGetProperty("spmlDocument", out var spmlDocElement));
            
            // Verify dictionary properties
            Assert.True(dictionaryElement.TryGetProperty("name", out var nameElement));
            Assert.Equal("Dictionary US", nameElement.GetString());
            
            // Verify signs array
            Assert.Equal(JsonValueKind.Array, signsElement.ValueKind);
            Assert.Equal(7, signsElement.GetArrayLength()); // Should have 7 valid signs
        }

        [Fact]
        public async Task DeleteSpmlDocument_AfterImport_ShouldRemoveDocument()
        {
            // Arrange - First import a document
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "test-dictionary.spml");

            var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
            importResponse.EnsureSuccessStatusCode();
            
            var importContent = await importResponse.Content.ReadAsStringAsync();
            var importResult = JsonSerializer.Deserialize<SpmlImportToCosmosResult>(importContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var documentId = importResult.SpmlDocumentEntity.Id;

            // Act - Delete the document
            var deleteResponse = await _client.DeleteAsync($"/api/spmlcosmos/{documentId}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(deleteContent);
            var root = jsonDoc.RootElement;
            
            Assert.True(root.TryGetProperty("success", out var successElement));
            Assert.True(successElement.GetBoolean());

            // Verify document is deleted
            var getResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetStats_AfterMultipleImports_ShouldReturnCorrectStats()
        {
            // Arrange - Import multiple documents
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            
            for (int i = 0; i < 3; i++)
            {
                var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
                content.Add(fileContent, "file", $"test-dictionary-{i}.spml");

                var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
                importResponse.EnsureSuccessStatusCode();
            }

            // Act
            var statsResponse = await _client.GetAsync("/api/spmlcosmos/stats");

            // Assert
            statsResponse.EnsureSuccessStatusCode();
            var statsContent = await statsResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(statsContent);
            var root = jsonDoc.RootElement;

            Assert.True(root.TryGetProperty("totalDocuments", out var totalDocsElement));
            Assert.True(root.TryGetProperty("totalEntries", out var totalEntriesElement));
            Assert.True(root.TryGetProperty("documentsByType", out var docsByTypeElement));

            Assert.Equal(3, totalDocsElement.GetInt32());
            Assert.Equal(30, totalEntriesElement.GetInt32()); // 10 entries per document * 3 documents

            Assert.True(docsByTypeElement.TryGetProperty("sgn", out var sgnCountElement));
            Assert.Equal(3, sgnCountElement.GetInt32());
        }

        [Fact]
        public async Task ImportSpml_WithInvalidFile_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidXml = "<invalid>xml content</invalid>";
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(invalidXml));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "invalid.spml");

            // Act
            var response = await _client.PostAsync("/api/spmlcosmos/import", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Failed to import", responseContent);
        }

        [Fact]
        public async Task GetSpmlDocument_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/spmlcosmos/non-existent-id");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ExportSpmlDocument_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/spmlcosmos/non-existent-id/export");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSpmlDocument_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.DeleteAsync("/api/spmlcosmos/non-existent-id");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task FullWorkflow_ImportExportConvertDelete_ShouldWorkEndToEnd()
        {
            // 1. Import
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "workflow-test.spml");
            content.Add(new StringContent("workflow-owner"), "ownerId");
            content.Add(new StringContent("Workflow test dictionary"), "description");
            content.Add(new StringContent("workflow,test"), "tags");

            var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
            importResponse.EnsureSuccessStatusCode();
            
            var importContent = await importResponse.Content.ReadAsStringAsync();
            var importResult = JsonSerializer.Deserialize<SpmlImportToCosmosResult>(importContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var documentId = importResult.SpmlDocumentEntity.Id;
            var document = importResult.SpmlDocumentEntity;

            // 2. Get
            var getResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}");
            getResponse.EnsureSuccessStatusCode();

            // 3. Export
            var exportResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}/export");
            exportResponse.EnsureSuccessStatusCode();
            var exportedXml = await exportResponse.Content.ReadAsStringAsync();
            Assert.Contains("<?xml version=\"1.0\"", exportedXml);

            // 4. Convert
            var convertContent = new StringContent(
                JsonSerializer.Serialize(document, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json");
            var convertResponse = await _client.PostAsync("/api/spmlcosmos/convert", convertContent);
            convertResponse.EnsureSuccessStatusCode();

            // 5. Stats
            var statsResponse = await _client.GetAsync("/api/spmlcosmos/stats");
            statsResponse.EnsureSuccessStatusCode();

            // 6. Delete
            var deleteResponse = await _client.DeleteAsync($"/api/spmlcosmos/{documentId}");
            deleteResponse.EnsureSuccessStatusCode();

            // 7. Verify deletion
            var getAfterDeleteResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        /// <summary>
        /// Helper class for deserializing import results
        /// </summary>
        public class SpmlImportToCosmosResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public SpmlDocumentEntity SpmlDocumentEntity { get; set; }
            public Dictionary Dictionary { get; set; }
            public List<Sign> Signs { get; set; }
            public string Error { get; set; }
        }
    }
}

