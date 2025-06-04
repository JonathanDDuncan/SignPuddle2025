using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignPuddle.API.Data;
using SignPuddle.API.Data.Repositories;
using SignPuddle.API.Models;
using SignPuddle.API.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SignPuddle.API.Tests.Performance
{
    /// <summary>
    /// Performance tests for SPML CosmosDB operations
    /// </summary>
    public class SpmlPerformanceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ISpmlPersistenceService _spmlPersistenceService;
        private readonly ISpmlRepository _repository;
        private readonly ITestOutputHelper _output;
        private readonly string _testDataPath;

        public SpmlPerformanceTests(ITestOutputHelper output)
        {
            _output = output;

            // Setup in-memory database for testing
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Initialize services
            var spmlImportService = new SpmlImportService();
            _repository = new SpmlRepository(_context);
            _spmlPersistenceService = new SpmlPersistenceService(spmlImportService, _repository);

            _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
        }

        [Fact]
        public async Task ImportLargeSpmlDocument_ShouldCompleteWithinReasonableTime()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var stopwatch = Stopwatch.StartNew();

            // Act
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(
                xmlContent, 
                "performance-test-owner", 
                "Performance test dictionary",
                new List<string> { "performance", "test" });

            stopwatch.Stop();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.SpmlDocumentEntity);
            
            // Should complete within 5 seconds for the test file
            var maxExpectedTime = TimeSpan.FromSeconds(5);
            _output.WriteLine($"Import completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            Assert.True(stopwatch.Elapsed < maxExpectedTime, 
                $"Import took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxExpectedTime.TotalMilliseconds}ms");
        }

        [Fact]
        public async Task BulkImport_MultipleDictionaries_ShouldScaleWell()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var importCount = 10;
            var maxTimePerImport = TimeSpan.FromSeconds(3);
            var results = new List<(TimeSpan Duration, bool Success)>();

            // Act
            for (int i = 0; i < importCount; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                
                var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(
                    xmlContent, 
                    $"bulk-test-owner-{i}", 
                    $"Bulk test dictionary {i}",
                    new List<string> { "bulk", "test", $"batch-{i}" });

                stopwatch.Stop();
                results.Add((stopwatch.Elapsed, result.Success));
                
                _output.WriteLine($"Import {i + 1}/{importCount} completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            }

            // Assert
            Assert.All(results, r => Assert.True(r.Success, "All imports should succeed"));
            Assert.All(results, r => Assert.True(r.Duration < maxTimePerImport, 
                $"Import took {r.Duration.TotalMilliseconds}ms, expected less than {maxTimePerImport.TotalMilliseconds}ms"));

            var averageTime = TimeSpan.FromMilliseconds(results.Average(r => r.Duration.TotalMilliseconds));
            _output.WriteLine($"Average import time: {averageTime.TotalMilliseconds}ms");

            // Verify all documents were saved
            var allDocuments = await _repository.GetAllSpmlDocumentsAsync();
            Assert.Equal(importCount, allDocuments.Count());
        }

        [Fact]
        public async Task QueryPerformance_GetById_ShouldBeEfficient()
        {
            // Arrange - Import some documents first
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var documentIds = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, $"query-test-owner-{i}");
                documentIds.Add(result.SpmlDocumentEntity.Id);
            }

            var maxQueryTime = TimeSpan.FromMilliseconds(500);

            // Act & Assert
            foreach (var documentId in documentIds)
            {
                var stopwatch = Stopwatch.StartNew();
                var document = await _repository.GetSpmlDocumentByIdAsync(documentId);
                stopwatch.Stop();

                _output.WriteLine($"GetById query completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
                
                Assert.NotNull(document);
                Assert.True(stopwatch.Elapsed < maxQueryTime, 
                    $"Query took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxQueryTime.TotalMilliseconds}ms");
            }
        }

        [Fact]
        public async Task QueryPerformance_GetByType_ShouldScaleWithDocumentCount()
        {
            // Arrange - Import multiple documents
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var documentCount = 20;

            for (int i = 0; i < documentCount; i++)
            {
                await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, $"scale-test-owner-{i}");
            }

            var maxQueryTime = TimeSpan.FromSeconds(2);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var sgnDocuments = await _repository.GetSpmlDocumentsByTypeAsync("sgn");
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"GetByType query for {documentCount} documents completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            
            Assert.Equal(documentCount, sgnDocuments.Count());
            Assert.True(stopwatch.Elapsed < maxQueryTime, 
                $"Query took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxQueryTime.TotalMilliseconds}ms");
        }

        [Fact]
        public async Task QueryPerformance_GetByOwner_ShouldBeEfficient()
        {
            // Arrange - Import documents for different owners
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var targetOwner = "performance-target-owner";
            var documentsPerOwner = 10;

            // Import documents for target owner
            for (int i = 0; i < documentsPerOwner; i++)
            {
                await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, targetOwner);
            }

            // Import documents for other owners
            for (int i = 0; i < documentsPerOwner; i++)
            {
                await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, $"other-owner-{i}");
            }

            var maxQueryTime = TimeSpan.FromSeconds(1);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var ownerDocuments = await _repository.GetSpmlDocumentsByOwnerAsync(targetOwner);
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"GetByOwner query completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            
            Assert.Equal(documentsPerOwner, ownerDocuments.Count());
            Assert.All(ownerDocuments, doc => Assert.Equal(targetOwner, doc.OwnerId));
            Assert.True(stopwatch.Elapsed < maxQueryTime, 
                $"Query took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxQueryTime.TotalMilliseconds}ms");
        }

        [Fact]
        public async Task ExportPerformance_LargeDocument_ShouldBeEfficient()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, "export-test-owner");
            var documentId = result.SpmlDocumentEntity.Id;

            var maxExportTime = TimeSpan.FromSeconds(2);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var exportedXml = await _repository.ExportSpmlDocumentAsXmlAsync(documentId);
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Export completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            
            Assert.NotNull(exportedXml);
            Assert.Contains("<?xml version=\"1.0\"", exportedXml);
            Assert.True(stopwatch.Elapsed < maxExportTime, 
                $"Export took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxExportTime.TotalMilliseconds}ms");
        }

        [Fact]
        public async Task ConversionPerformance_SpmlToEntities_ShouldBeEfficient()
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, "conversion-test-owner");
            var document = result.SpmlDocumentEntity;

            var maxConversionTime = TimeSpan.FromSeconds(3);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var conversionResult = await _spmlPersistenceService.ConvertSpmlDocumentToEntitiesAsync(document);
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Conversion completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            
            Assert.NotNull(conversionResult);
            Assert.NotNull(conversionResult.Dictionary);
            Assert.NotNull(conversionResult.Signs);
            Assert.True(stopwatch.Elapsed < maxConversionTime, 
                $"Conversion took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxConversionTime.TotalMilliseconds}ms");
        }

        [Fact]
        public async Task StatsPerformance_WithManyDocuments_ShouldBeEfficient()
        {
            // Arrange - Import multiple documents
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var documentCount = 15;

            for (int i = 0; i < documentCount; i++)
            {
                await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, $"stats-test-owner-{i}");
            }

            var maxStatsTime = TimeSpan.FromSeconds(1);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var stats = await _repository.GetSpmlDocumentStatsAsync();
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Stats calculation completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            
            Assert.Equal(documentCount, stats.TotalDocuments);
            Assert.Equal(documentCount * 10, stats.TotalEntries); // 10 entries per test document
            Assert.True(stopwatch.Elapsed < maxStatsTime, 
                $"Stats calculation took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxStatsTime.TotalMilliseconds}ms");
        }

        [Fact]
        public async Task DeletePerformance_MultipleDocuments_ShouldBeEfficient()
        {
            // Arrange - Import documents to delete
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var documentIds = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, $"delete-test-owner-{i}");
                documentIds.Add(result.SpmlDocumentEntity.Id);
            }

            var maxDeleteTime = TimeSpan.FromMilliseconds(200);

            // Act & Assert
            foreach (var documentId in documentIds)
            {
                var stopwatch = Stopwatch.StartNew();
                var deleteResult = await _repository.DeleteSpmlDocumentAsync(documentId);
                stopwatch.Stop();

                _output.WriteLine($"Delete completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
                
                Assert.True(deleteResult);
                Assert.True(stopwatch.Elapsed < maxDeleteTime, 
                    $"Delete took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxDeleteTime.TotalMilliseconds}ms");
            }

            // Verify all documents were deleted
            var remainingDocuments = await _repository.GetAllSpmlDocumentsAsync();
            Assert.Empty(remainingDocuments);
        }

        [Fact]
        public async Task MemoryUsage_BulkOperations_ShouldNotExceedLimits()
        {
            // This test helps identify potential memory leaks
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var initialMemory = GC.GetTotalMemory(true);

            // Perform bulk operations
            for (int i = 0; i < 20; i++)
            {
                var result = await _spmlPersistenceService.ImportAndSaveSpmlAsync(xmlContent, $"memory-test-owner-{i}");
                
                // Force garbage collection every 5 operations
                if (i % 5 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }

            var finalMemory = GC.GetTotalMemory(true);
            var memoryIncrease = finalMemory - initialMemory;

            _output.WriteLine($"Memory usage increased by {memoryIncrease / 1024 / 1024} MB");
            
            // Memory increase should be reasonable (less than 100 MB for test operations)
            var maxAcceptableIncrease = 100 * 1024 * 1024; // 100 MB
            Assert.True(memoryIncrease < maxAcceptableIncrease, 
                $"Memory usage increased by {memoryIncrease / 1024 / 1024} MB, expected less than {maxAcceptableIncrease / 1024 / 1024} MB");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ConcurrentImports_ShouldHandleParallelOperations(int concurrentCount)
        {
            // Arrange
            var xmlContent = await File.ReadAllTextAsync(_testDataPath);
            var tasks = new List<Task<SpmlImportToCosmosResult>>();

            var maxConcurrentTime = TimeSpan.FromSeconds(10);
            var stopwatch = Stopwatch.StartNew();

            // Act - Start concurrent imports
            for (int i = 0; i < concurrentCount; i++)
            {
                var taskIndex = i; // Capture for closure
                var task = _spmlPersistenceService.ImportAndSaveSpmlAsync(
                    xmlContent, 
                    $"concurrent-test-owner-{taskIndex}",
                    $"Concurrent test dictionary {taskIndex}",
                    new List<string> { "concurrent", "test", $"task-{taskIndex}" });
                
                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Concurrent imports ({concurrentCount}) completed in {stopwatch.Elapsed.TotalMilliseconds}ms");
            
            Assert.All(results, result => Assert.True(result.Success, "All concurrent imports should succeed"));
            Assert.True(stopwatch.Elapsed < maxConcurrentTime, 
                $"Concurrent operations took {stopwatch.Elapsed.TotalMilliseconds}ms, expected less than {maxConcurrentTime.TotalMilliseconds}ms");

            // Verify all documents were saved with unique IDs
            var allDocuments = await _repository.GetAllSpmlDocumentsAsync();
            var uniqueIds = allDocuments.Select(d => d.Id).Distinct().Count();
            Assert.Equal(concurrentCount, uniqueIds);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}


