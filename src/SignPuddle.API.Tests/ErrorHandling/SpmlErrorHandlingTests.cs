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
    /// Simplified tests for error handling and edge cases in SPML CosmosDB functionality
    /// </summary>
    public class SpmlErrorHandlingTests : IDisposable
    {
        private readonly IServiceProvider _serviceProviderFactory;
        private readonly ISpmlPersistenceService _spmlPersistenceService;

        public SpmlErrorHandlingTests()
        {
            var services = new ServiceCollection();
            var dbName = Guid.NewGuid().ToString();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: dbName), ServiceLifetime.Transient);

            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            services.AddTransient<ISpmlImportService, SpmlImportService>();
            services.AddTransient<ISpmlRepository, SpmlRepository>();
            services.AddTransient<ISpmlPersistenceService, SpmlPersistenceService>();

            _serviceProviderFactory = services.BuildServiceProvider();
            _spmlPersistenceService = _serviceProviderFactory.GetRequiredService<ISpmlPersistenceService>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ImportAndSaveSpmlAsync_InvalidOrEmptyXml_ShouldReturnFailure(string invalidXml)
        {
            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(invalidXml);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.SpmlDocumentEntity);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_MalformedXml_ShouldReturnFailure()
        {
            // Arrange
            var malformedXml = @"<spml><entry>Missing closing tags</entry>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(malformedXml);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.SpmlDocumentEntity);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_NonSpmlXml_ShouldReturnFailure()
        {
            // Arrange
            var nonSpmlXml = @"<book><title>Not an SPML document</title></book>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(nonSpmlXml);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.SpmlDocumentEntity);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_EmptySpml_ShouldSucceedWithZeroEntries()
        {
            // Arrange
            var emptySpml = @"<?xml version=""1.0""?><spml><meta><title>Empty Dictionary</title></meta></spml>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(emptySpml);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.Equal(0, result.SpmlDocumentEntity.EntryCount);
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_SpecialCharacters_ShouldSucceed()
        {
            // Arrange - SPML with special characters and Unicode
            var specialCharsSpml = @"<?xml version=""1.0"" encoding=""UTF-8""?><spml><meta><title>éñ中文🤟</title></meta><entry id=""1""><term>café</term></entry></spml>";

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(specialCharsSpml);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            Assert.Contains("éñ中文🤟", result.SpmlDocumentEntity.DictionaryName);
        }

        [Fact]
        public async Task SaveSpmlDocumentAsync_NullArguments_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _spmlPersistenceService.SaveSpmlDocumentAsync(null, "test xml"));

            var spmlDocument = new SpmlDocument { Type = "sgn", PuddleId = 1, Terms = new List<string> { "Test" }, Entries = new List<SpmlEntry>() };
            await Assert.ThrowsAsync<ArgumentNullException>(() => _spmlPersistenceService.SaveSpmlDocumentAsync(spmlDocument, null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetOrExportOrDeleteSpmlDocumentAsync_InvalidId_ShouldReturnNullOrFalse(string invalidId)
        {
            // Act & Assert
            Assert.Null(await _spmlPersistenceService.GetSpmlDocumentAsync(invalidId));
            Assert.Null(await _spmlPersistenceService.ExportSpmlDocumentAsXmlAsync(invalidId));
            Assert.False(await _spmlPersistenceService.DeleteSpmlDocumentAsync(invalidId));
        }

        [Fact]
        public async Task ConvertSpmlDocumentToEntitiesAsync_NullOrCorrupted_ShouldThrowOrFail()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(null));

            var corruptedEntity = new SpmlDocumentEntity { Id = Guid.NewGuid().ToString(), PartitionKey = "sgn", DocumentType = "spml", SpmlDocument = null, OriginalXml = "<test>corrupted</test>", SavedAt = DateTime.UtcNow };
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(corruptedEntity));
            Assert.Contains("SpmlDocument property cannot be null", ex.Message);
        }

        [Fact]
        public async Task Repository_DatabaseConnectionIssues_ShouldThrow()
        {
            // This test simulates database connectivity issues
            // For in-memory database, we'll dispose the context to simulate connection loss
            
            // Resolve a context specifically for this test to dispose
            using var scope = _serviceProviderFactory.CreateScope();
            var contextToDispose = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repositoryWithDisposedContext = new SpmlRepository(contextToDispose);
            
            contextToDispose.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => repositoryWithDisposedContext.GetAllSpmlDocumentsAsync());
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_DuplicateOrMissingFields_ShouldSucceed()
        {
            // Arrange - SPML with duplicate entry IDs
            var duplicateIdSpml = @"<spml><meta><title>Duplicate IDs Test</title></meta><entry id=""1""><term>first</term></entry><entry id=""1""><term>second</term></entry></spml>";

            // Act
            var result1 = await _spmlPersistenceService.ImportAndSaveSpmlAsync(duplicateIdSpml);

            // Assert
            Assert.True(result1.Success);
            Assert.NotNull(result1.SpmlDocumentEntity);

            // Arrange - SPML missing required fields
            var missingFieldsSpml = @"<spml><entry><sign><hbsym>B10</hbsym></sign></entry></spml>";

            // Act
            var result2 = await _spmlPersistenceService.ImportAndSaveSpmlAsync(missingFieldsSpml);

            // Assert
            Assert.True(result2.Success);
            Assert.NotNull(result2.SpmlDocumentEntity);
        }

        [Fact]
        public async Task Repository_LargeDataSets_ShouldNotCauseMemoryIssues()
        {
            // Arrange - Create many entities
            var entities = new List<SpmlDocumentEntity>();
            var repository = _serviceProviderFactory.GetRequiredService<ISpmlRepository>(); 

            for (int i = 0; i < 100; i++)
            {
                var spmlDocument = new SpmlDocument
                {
                    Type = "sgn",
                    PuddleId = i,
                    Terms = new List<string> { $"Test Dictionary {i}" },
                    Entries = Enumerable.Range(1, 10).Select(j => new SpmlEntry
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
            
            foreach (var entity in entities)
                await repository.SaveAsync(entity); 

            var finalMemory = GC.GetTotalMemory(true);
            // Memory increase should be reasonable
            var maxAcceptableIncrease = 50 * 1024 * 1024; // 50 MB
            Assert.True(finalMemory - initialMemory < maxAcceptableIncrease, 
                $"Memory usage increased by {finalMemory - initialMemory} bytes during large dataset operations");
        }

        [Fact]
        public async Task ImportAndSaveSpmlAsync_ConcurrentOperations_ShouldNotCauseDataCorruption()
        {
            // Arrange
            var spmlXml = @"<spml><meta><title>Concurrent Test</title></meta><entry id=""1""><term>concurrent</term></entry></spml>";
            var tasks = new List<Task<SpmlImportToCosmosResult>>();
            var concurrentDbName = Guid.NewGuid().ToString();
            for (int i = 0; i < 5; i++)
            {
                var taskIndex = i;
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
                    return await persistenceService.ImportAndSaveSpmlAsync(spmlXml, $"concurrent-owner-{taskIndex}");
                });
                tasks.Add(task);
            }
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result => Assert.True(result.Success));
            
            // Verify data using a final, separate context pointing to the same database
            var verificationServices = new ServiceCollection();
            verificationServices.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(concurrentDbName));
            verificationServices.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            verificationServices.AddTransient<ISpmlRepository, SpmlRepository>();
            await using var verificationProvider = verificationServices.BuildServiceProvider();
            
            var repository = verificationProvider.GetRequiredService<ISpmlRepository>();
            var allDocuments = await repository.GetAllSpmlDocumentsAsync();
            
            Assert.Equal(5, allDocuments.Count());
            var uniqueIds = allDocuments.Select(d => d.Id).Distinct().Count();
            Assert.Equal(5, uniqueIds); // All documents should have unique IDs
        }

        [Fact]
        public void SpmlDocumentEntity_FromSpmlDocument_NullInputs_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => SpmlDocumentEntity.FromSpmlDocument(null, "test xml", "owner"));
            Assert.Throws<ArgumentNullException>(() => SpmlDocumentEntity.FromSpmlDocument(new SpmlDocument(), null, "owner"));
        }

        [Fact]
        public void SpmlDocumentEntity_FromSpmlDocument_EmptySpmlDocument_ShouldUseDefaults()
        {
            // Arrange
            var emptySpmlDocument = new SpmlDocument { Type = null, PuddleId = 0, Terms = null, Entries = null };

            // Act
            var entity = SpmlDocumentEntity.FromSpmlDocument(emptySpmlDocument, "<test>xml</test>", "test-owner");

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("unknown", entity.PartitionKey);
            Assert.Equal("SPML Dictionary: Unknown", entity.Description);
            Assert.Single(entity.Tags);
            Assert.Equal(0, entity.EntryCount);
            Assert.Equal(0, entity.PuddleId);
        }

        [Fact]
        public async Task ExportAsXml_CorruptedDocument_ShouldReturnNull()
        {
            // Arrange - Create document with corrupted SpmlDocument
            var repository = _serviceProviderFactory.GetRequiredService<ISpmlRepository>(); 
            var corruptedEntity = new SpmlDocumentEntity
            {
                Id = Guid.NewGuid().ToString(),
                PartitionKey = "sgn",
                DocumentType = "spml",
                SpmlDocument = null,
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
            if (_serviceProviderFactory is IDisposable disposableFactory)
                disposableFactory.Dispose();
        }
    }
}



