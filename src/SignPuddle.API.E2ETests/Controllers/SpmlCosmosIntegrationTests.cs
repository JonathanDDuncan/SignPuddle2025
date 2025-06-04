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

namespace SignPuddle.API.E2ETests.Controllers
{
    /// <summary>
    /// Integration tests for SPML CosmosDB functionality
    /// </summary>
    public class SpmlCosmosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly string _testDataPath; 
        private static readonly string InMemoryDbName = "SpmlCosmosIntegrationTestsDb";

        public SpmlCosmosIntegrationTests(WebApplicationFactory<Program> factory)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(databaseName: InMemoryDbName));

                    // Clear the database before each test
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                });
            });

            _client = _factory.CreateClient();
            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        [Fact]
        public async Task ImportExportConvertDeleteWorkflow_ShouldWorkEndToEnd()
        {
            // Import
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "workflow-test.spml");
            content.Add(new StringContent("workflow-owner"), "ownerId");
            content.Add(new StringContent("Workflow test dictionary"), "description");
            content.Add(new StringContent("workflow,test"), "tags");

            var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
            if (!importResponse.IsSuccessStatusCode)
            {
                var errorContent = await importResponse.Content.ReadAsStringAsync();
                throw new Exception($"Import failed: {importResponse.StatusCode} - {errorContent}");
            }
            var importContent = await importResponse.Content.ReadAsStringAsync();
            SpmlImportToCosmosResult importResult = null;
            try
            {
                importResult = JsonSerializer.Deserialize<SpmlImportToCosmosResult>(importContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}\nRaw JSON: {importContent}");
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "importContent.json"), importContent);
                throw;
            }

            var documentId = importResult.SpmlDocumentEntity.Id;
            var document = importResult.SpmlDocumentEntity;

            // Get
            var getResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}");
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getDoc = JsonSerializer.Deserialize<SpmlDocumentEntity>(getContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Assert.NotNull(getDoc);
            Assert.Equal(documentId, getDoc.Id);

            // Export
            var exportResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}/export");
            exportResponse.EnsureSuccessStatusCode();
            var exportedXml = await exportResponse.Content.ReadAsStringAsync();
            Assert.Contains("<?xml version=\"1.0\"", exportedXml);

            // Convert
            var convertContent = new StringContent(
                JsonSerializer.Serialize(document, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json");
            var convertResponse = await _client.PostAsync("/api/spmlcosmos/convert", convertContent);
            convertResponse.EnsureSuccessStatusCode();
            var convertResponseContent = await convertResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(convertResponseContent);
            var root = jsonDoc.RootElement;
            Assert.True(root.TryGetProperty("dictionary", out var dictionaryElement));
            Assert.True(root.TryGetProperty("signs", out var signsElement));
            Assert.True(root.TryGetProperty("spmlDocument", out var spmlDocElement));

            // Delete
            var deleteResponse = await _client.DeleteAsync($"/api/spmlcosmos/{documentId}");
            deleteResponse.EnsureSuccessStatusCode();
            var getAfterDeleteResponse = await _client.GetAsync($"/api/spmlcosmos/{documentId}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
        }

        [Fact]
        public async Task ImportSpml_WithInvalidFile_ShouldReturnBadRequest()
        {
            var invalidXml = "<invalid>xml content</invalid>";
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(invalidXml));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            content.Add(fileContent, "file", "invalid.spml");

            var response = await _client.PostAsync("/api/spmlcosmos/import", content);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Failed to import", responseContent);
        }

        [Fact]
        public async Task GetStats_AfterMultipleImports_ShouldReturnCorrectStats()
        {
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            int importSuccessCount = 0;
            int importEntryCount = 0;
            for (int i = 0; i < 3; i++)
            {
                var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xmlContent));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
                content.Add(fileContent, "file", $"test-dictionary-{i}.spml");
                var importResponse = await _client.PostAsync("/api/spmlcosmos/import", content);
                if (importResponse.IsSuccessStatusCode)
                {
                    importSuccessCount++;
                    var importContent = await importResponse.Content.ReadAsStringAsync();
                    using var importJson = JsonDocument.Parse(importContent);
                    if (importJson.RootElement.TryGetProperty("spmlDocumentEntity", out var entity) && entity.TryGetProperty("entryCount", out var entryCount))
                        importEntryCount += entryCount.GetInt32();
                }
                Assert.True(importResponse.IsSuccessStatusCode || importResponse.StatusCode == System.Net.HttpStatusCode.BadRequest, $"Unexpected status: {importResponse.StatusCode}");
            }

            var statsResponse = await _client.GetAsync("/api/spmlcosmos/stats");
            statsResponse.EnsureSuccessStatusCode();
            var statsContent = await statsResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(statsContent);
            var root = jsonDoc.RootElement;

            Assert.True(root.TryGetProperty("totalDocuments", out var totalDocsElement));
            Assert.True(root.TryGetProperty("totalEntries", out var totalEntriesElement));
            Assert.True(root.TryGetProperty("documentsByType", out var docsByTypeElement));
            Assert.Equal(importSuccessCount, totalDocsElement.GetInt32());
            Assert.Equal(importEntryCount, totalEntriesElement.GetInt32());
            Assert.True(docsByTypeElement.TryGetProperty("sgn", out var sgnCountElement));
            Assert.Equal(importSuccessCount, sgnCountElement.GetInt32());
            Assert.True(docsByTypeElement.ValueKind == JsonValueKind.Object);
            var docTypeNames = docsByTypeElement.EnumerateObject().Select(p => p.Name).ToList();
            File.WriteAllText(Path.Combine("C:\\Code\\SignWriting\\SignPuddle", "documentsByType.json"), docsByTypeElement.ToString());
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public class SpmlImportToCosmosResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public SpmlDocumentEntity SpmlDocumentEntity { get; set; }
            public Dictionary Dictionary { get; set; }
            public List<Sign> Signs { get; set; }
            public string ErrorMessage { get; set; } // Match API model
        }
    }
}
