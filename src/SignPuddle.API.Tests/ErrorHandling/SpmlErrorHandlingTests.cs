using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

namespace SignPuddle.API.Tests.ErrorHandling
{
    /// <summary>
    /// Tests for error handling and edge cases in SPML CosmosDB functionality
    /// </summary>
    public class SpmlErrorHandlingTests : IDisposable
    {
        private readonly IServiceProvider _serviceProviderFactory;
        private readonly ISpmlPersistenceService _spmlPersistenceService; // For tests not needing specific isolation

        public SpmlErrorHandlingTests()
        {
            var services = new ServiceCollection();
            var dbName = Guid.NewGuid().ToString(); // Unique DB name per test class instance

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: dbName), ServiceLifetime.Transient);

            // Register NullLoggers for all services that require ILogger
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            
            services.AddTransient<ISpmlImportService, SpmlImportService>();
            services.AddTransient<ISpmlRepository, SpmlRepository>();
            services.AddTransient<ISpmlPersistenceService, SpmlPersistenceService>();

            _serviceProviderFactory = services.BuildServiceProvider();
            
            // Resolve a persistence service instance for general use in tests
            // Tests requiring specific contexts (like the concurrent one) will resolve their own.
            _spmlPersistenceService = _serviceProviderFactory.GetRequiredService<ISpmlPersistenceService>();
        }

        [Theory]
        [InlineData(null, "Value cannot be null. (Parameter 'spmlContent')")]
        [InlineData("", "SPML content cannot be empty or whitespace. (Parameter 'spmlContent')")]
        [InlineData("   ", "SPML content cannot be empty or whitespace. (Parameter 'spmlContent')")]
        public async Task ImportAndSaveSpmlAsync_WithInvalidXml_ShouldReturnFailure(string invalidXml, string expectedContainedMessage)
        {
            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(invalidXml);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.SpmlDocumentEntity);
            Assert.Null(result.Dictionary);
            Assert.Null(result.Signs);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("Failed to import SPML data.", result.ErrorMessage);
            Assert.Contains(expectedContainedMessage, result.ErrorMessage);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithMalformedXml_ShouldReturnFailure()
        {
            // Arrange
            var malformedXml = @"<?xml version=""1.0""?><spml><entry>Missing closing tags</entry></spml>"; // Corrected XML

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(malformedXml);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.SpmlDocumentEntity);
            Assert.NotNull(result.ErrorMessage);
            // Expecting a message indicating XML format issue
            Assert.Contains("Failed to import SPML data. Invalid XML format", result.ErrorMessage);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithNonSpmlXml_ShouldReturnFailure()
        {
            // Arrange
            var nonSpmlXml = @"<?xml version=""1.0""?>
                <book>
                    <title>Not an SPML document</title>
                    <author>Test Author</author>
                </book>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(nonSpmlXml);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.SpmlDocumentEntity);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithEmptySpml_ShouldHandleGracefully()
        {
            // Arrange
            var emptySpml = @"<?xml version=""1.0""?>
                <!DOCTYPE spml SYSTEM ""http://www.signbank.org/spml/spml.dtd"">
                <spml type=""sgn"" puddle=""999"" country=""test"" language=""test"">
                    <meta>
                        <title>Empty Dictionary</title>
                    </meta>
                </spml>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(emptySpml);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.Equal(0, result.SpmlDocumentEntity.EntryCount);
            Assert.Equal("Empty Dictionary", result.SpmlDocumentEntity.DictionaryName);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithVeryLongContent_ShouldHandleGracefully()
        {
            // Arrange - Create SPML with very long content
            var longTitle = new string('A', 10000); // 10K characters
            var longSpml = $@"<?xml version=""1.0""?>
                <!DOCTYPE spml SYSTEM ""http://www.signbank.org/spml/spml.dtd"">
                <spml type=""sgn"" puddle=""999"" country=""test"" language=""test"">
                    <meta>
                        <title>{longTitle}</title>
                    </meta>
                    <entry id=""1"">
                        <term>test</term>
                        <sign>
                            <hbsym>B10</hbsym>
                        </sign>
                    </entry>
                </spml>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(longSpml);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.Equal(longTitle, result.SpmlDocumentEntity.DictionaryName);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithSpecialCharacters_ShouldHandleGracefully()
        {
            // Arrange - SPML with special characters and Unicode
            var specialCharsSpml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <!DOCTYPE spml SYSTEM ""http://www.signbank.org/spml/spml.dtd"">
                <spml type=""sgn"" puddle=""999"" country=""test"" language=""test"">
                    <meta>
                        <title>Special Characters: éñ中文🤟&lt;&gt;&amp;""'</title>
                    </meta>
                    <entry id=""1"">
                        <term>café</term>
                        <sign>
                            <hbsym>B10</hbsym>
                        </sign>
                    </entry>
                    <entry id=""2"">
                        <term>中文</term>
                        <sign>
                            <hbsym>B11</hbsym>
                        </sign>
                    </entry>
                </spml>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(specialCharsSpml);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.Contains("éñ中文🤟", result.SpmlDocumentEntity.DictionaryName);
        }

        [Fact]
        public async Task SaveSpmlDocumentAsync_WithNullDocument_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _spmlPersistenceService.SaveSpmlDocumentAsync(null, "test xml"));
        }

        [Fact]
        public async Task SaveSpmlDocumentAsync_WithNullXml_ShouldThrowException()
        {
            // Arrange
            var spmlDocument = new SpmlDocument
            {
                Type = "sgn",
                PuddleId = 1,
                Terms = new List<string> { "Test" },
                Entries = new List<SpmlEntry>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetSpmlDocumentAsync_WithInvalidId_ShouldReturnNull(string invalidId)
        {
            // Act
            var result = await _spmlPersistenceService.GetSpmlDocumentAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExportSpmlDocumentAsXmlAsync_WithInvalidId_ShouldReturnNull(string invalidId)
        {
            // Act
            var result = await _spmlPersistenceService.ExportSpmlDocumentAsXmlAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteSpmlDocumentAsync_WithInvalidId_ShouldReturnFalse(string invalidId)
        {
            // Act
            var result = await _spmlPersistenceService.DeleteSpmlDocumentAsync(invalidId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ConvertSpmlDocumentToEntitiesAsync_WithNullDocument_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(null));
        }

        [Fact]
        public async Task ConvertSpmlDocumentToEntitiesAsync_WithCorruptedDocument_ShouldHandleGracefully()
        {
            // Arrange - Create a document with null SpmlDocument property
            var corruptedEntity = new SpmlDocumentEntity
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = "sgn",
                DocumentType = "spml",
                SpmlDocument = null, // This would normally cause issues
                OriginalXml = "<test>corrupted</test>",
                SavedAt = DateTime.UtcNow
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(corruptedEntity));
            
            Assert.Contains("SpmlDocument property cannot be null", exception.Message);
        }

        [Fact]
        public async Task Repository_DatabaseConnectionIssues_ShouldHandleGracefully()
        {
            // This test simulates database connectivity issues
            // For in-memory database, we'll dispose the context to simulate connection loss
            
            // Resolve a context specifically for this test to dispose
            using var scope = _serviceProviderFactory.CreateScope();
            var contextToDispose = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repositoryWithDisposedContext = new SpmlRepository(contextToDispose); // Corrected constructor
            
            contextToDispose.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(
                () => repositoryWithDisposedContext.GetAllSpmlDocumentsAsync());
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithDuplicateEntryIds_ShouldHandleGracefully()
        {
            // Arrange - SPML with duplicate entry IDs
            var duplicateIdSpml = @"<?xml version=""1.0""?>
                <!DOCTYPE spml SYSTEM ""http://www.signbank.org/spml/spml.dtd"">
                <spml type=""sgn"" puddle=""999"" country=""test"" language=""test"">
                    <meta>
                        <title>Duplicate IDs Test</title>
                    </meta>
                    <entry id=""1"">
                        <term>first</term>
                        <sign>
                            <hbsym>B10</hbsym>
                        </sign>
                    </entry>
                    <entry id=""1"">
                        <term>second</term>
                        <sign>
                            <hbsym>B11</hbsym>
                        </sign>
                    </entry>
                </spml>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(duplicateIdSpml);

            // Assert
            // Should handle duplicates gracefully (likely keeping the last one or both)
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_WithMissingRequiredFields_ShouldHandleGracefully()
        {
            // Arrange - SPML missing required fields
            var missingFieldsSpml = @"<?xml version=""1.0""?>
                <spml>
                    <entry>
                        <sign>
                            <hbsym>B10</hbsym>
                        </sign>
                    </entry>
                </spml>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(missingFieldsSpml);

            // Assert
            // Should handle missing fields gracefully with default values
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
        }

        [Fact]
        public async Task Repository_LargeDataSets_ShouldNotCauseMemoryIssues()
        {
            // Arrange - Create many entities
            var entities = new List<SpmlDocumentEntity>();
            var repository = _serviceProviderFactory.GetRequiredService<ISpmlRepository>(); 

            for (int i = 0; i < 1000; i++)
            {
                var spmlDocument = new SpmlDocument
                {
                    Type = "sgn",
                    PuddleId = i,
                    Terms = new List<string> { $"Test Dictionary {i}" },
                    Entries = Enumerable.Range(1, 100).Select(j => new SpmlEntry
                    {
                        Id = j,
                        Terms = new List<string> { $"term-{i}-{j}" }
                    }).ToList()
                };

                var entity = SpmlDocumentEntity.FromSpmlDocument(spmlDocument, $"<test>{i}</test>", $"owner-{i}");
                entities.Add(entity);
            }

            // Act
            var initialMemory = GC.GetTotalMemory(true);
            
            foreach (var entity in entities.Take(100)) // Limit to avoid test timeout
            {
                await repository.SaveAsync(entity); 
            }

            var finalMemory = GC.GetTotalMemory(true);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert
            // Memory increase should be reasonable
            var maxAcceptableIncrease = 50 * 1024 * 1024; // 50 MB
            Assert.True(memoryIncrease < maxAcceptableIncrease, 
                $"Memory usage increased by {memoryIncrease / 1024 / 1024} MB during large dataset operations");
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_ConcurrentOperations_ShouldNotCauseDataCorruption()
        {
            // Arrange
            var spmlXml = @"<?xml version=""1.0""?>
                <!DOCTYPE spml SYSTEM ""http://www.signbank.org/spml/spml.dtd"">
                <spml type=""sgn"" puddle=""999"" country=""test"" language=""test"">
                    <meta>
                        <title>Concurrent Test</title>
                    </meta>
                    <entry id=""1"">
                        <term>concurrent</term>
                        <sign>
                            <hbsym>B10</hbsym>
                        </sign>
                    </entry>
                </spml>";

            var tasks = new List<Task<SpmlImportToCosmosResult>>();
            var concurrentDbName = Guid.NewGuid().ToString(); // Shared DB name for this test's operations

            // Act - Start multiple concurrent imports
            for (int i = 0; i < 10; i++)
            {
                var taskIndex = i; // Capture loop variable for closure
                var task = Task.Run(async () =>
                {
                    var services = new ServiceCollection();
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(databaseName: concurrentDbName), ServiceLifetime.Transient);

                    services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
                    services.AddTransient<ISpmlImportService, SpmlImportService>();
                    services.AddTransient<ISpmlRepository, SpmlRepository>();
                    services.AddTransient<ISpmlPersistenceService, SpmlPersistenceService>();
                    
                    await using var serviceProvider = services.BuildServiceProvider();
                    var persistenceService = serviceProvider.GetRequiredService<ISpmlPersistenceService>();
                    return await persistenceService.ImportAndSaveSpmlAsync(
                        spmlXml,
                        $"concurrent-owner-{taskIndex}");
                });
                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result => Assert.True(result.Success, $"One of the concurrent operations failed: {result.ErrorMessage ?? "No error message."}"));
            
            // Verify data using a final, separate context pointing to the same database
            var verificationServices = new ServiceCollection();
            verificationServices.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(concurrentDbName));
            verificationServices.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            verificationServices.AddTransient<ISpmlRepository, SpmlRepository>();
            await using var verificationProvider = verificationServices.BuildServiceProvider();
            
            var repository = verificationProvider.GetRequiredService<ISpmlRepository>();
            var allDocuments = await repository.GetAllSpmlDocumentsAsync();
            
            Assert.Equal(10, allDocuments.Count());
            var uniqueIds = allDocuments.Select(d => d.Id).Distinct().Count();
            Assert.Equal(10, uniqueIds); // All documents should have unique IDs
        }

        [Fact]
        public void SpmlDocumentEntity_FromSpmlDocument_WithNullInputs_ShouldHandleGracefully() 
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => SpmlDocumentEntity.FromSpmlDocument(null, "test xml", "owner"));

            Assert.Throws<ArgumentNullException>(
                () => SpmlDocumentEntity.FromSpmlDocument(new SpmlDocument(), null, "owner"));
        }

        [Fact]
        public void SpmlDocumentEntity_FromSpmlDocument_WithEmptySpmlDocument_ShouldUseDefaults() 
        {
            // Arrange
            var emptySpmlDocument = new SpmlDocument
            {
                Type = null,
                PuddleId = 0,
                Terms = null,
                Entries = null
            };

            // Act
            var entity = SpmlDocumentEntity.FromSpmlDocument(emptySpmlDocument, "<test>xml</test>", "test-owner");

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("unknown", entity.PartitionKey); // Should default when Type is null
            Assert.Equal("SPML Dictionary: Unknown", entity.Description);
            Assert.Single(entity.Tags); // Should have at least one default tag
            Assert.Equal(0, entity.EntryCount);
            Assert.Equal(0, entity.PuddleId);
        }

        [Fact]
        public async Task ExportAsXml_WithCorruptedDocument_ShouldHandleGracefully()
        {
            // Arrange - Create document with corrupted SpmlDocument
            var repository = _serviceProviderFactory.GetRequiredService<ISpmlRepository>(); 
            var corruptedEntity = new SpmlDocumentEntity
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = "sgn",
                DocumentType = "spml",
                SpmlDocument = null, // This is the corruption
                OriginalXml = "<test>corrupted</test>",
                SavedAt = DateTime.UtcNow
            };
            await repository.SaveAsync(corruptedEntity); 

            // Act
            var exportedXml = await repository.ExportSpmlDocumentAsXmlAsync(corruptedEntity.Id); 

            // Assert
            Assert.Null(exportedXml); // Expect null or specific error handling if implemented
        }

        public void Dispose()
        {
            // Dispose of the DbContext if it's an InMemory database
            // For other providers, this might not be necessary or might be handled differently.
            if (_serviceProviderFactory is IDisposable disposableFactory)
            {
                disposableFactory.Dispose();
            }
            // If _spmlPersistenceService or other resolved services implement IDisposable and need cleanup,
            // they should be handled here or by making the test class IAsyncLifetime if async cleanup is needed.
        }
    }
}



