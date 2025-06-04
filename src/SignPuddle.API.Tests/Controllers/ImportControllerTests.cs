using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using SignPuddle.API.Controllers;
using SignPuddle.API.Models;
using SignPuddle.API.Services;

namespace SignPuddle.API.Tests.Controllers
{
    public class ImportControllerTests
    {
        private readonly Mock<ISpmlImportService> _mockSpmlImportService;
        private readonly ImportController _controller;

        public ImportControllerTests()
        {
            _mockSpmlImportService = new Mock<ISpmlImportService>();
            _controller = new ImportController(_mockSpmlImportService.Object);
        }

        [Fact]
        public async Task ImportSpmlFile_WithNullFile_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.ImportSpmlFile(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("No file provided", badRequestResult.Value);
        }

        [Fact]
        public async Task ImportSpmlFile_WithEmptyFile_ShouldReturnBadRequest()
        {
            // Arrange
            var file = CreateMockFile("test.spml", "");

            // Act
            var result = await _controller.ImportSpmlFile(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("No file provided", badRequestResult.Value);
        }

        [Fact]
        public async Task ImportSpmlFile_WithNonSpmlFile_ShouldReturnBadRequest()
        {
            // Arrange
            var file = CreateMockFile("test.txt", "some content");

            // Act
            var result = await _controller.ImportSpmlFile(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("File must be an SPML file", badRequestResult.Value);
        }

        [Fact]
        public async Task ImportSpmlFile_WithValidFile_ShouldReturnImportResult()
        {
            // Arrange
            var xmlContent = GetTestSpmlContent();
            var file = CreateMockFile("test.spml", xmlContent);
            var ownerId = "user123";            var mockSpmlDocument = new SpmlDocument
            {
                PuddleId = 4,
                Terms = new List<string> { "Test Dictionary" },
                Entries = new List<SpmlEntry>
                {
                    new SpmlEntry { Id = 1, Terms = new List<string> { "AS123", "test" } }
                }
            };

            var mockDictionary = new Dictionary
            {
                Name = "Test Dictionary",
                Language = "sgn"
            };            var mockSigns = new List<SignPuddle.API.Models.Sign>
            {
                new SignPuddle.API.Models.Sign { Id = 1, Fsw = "AS123", Gloss = "test" }
            };

            _mockSpmlImportService.Setup(s => s.ParseSpmlAsync(It.IsAny<string>()))
                .ReturnsAsync(mockSpmlDocument);
            _mockSpmlImportService.Setup(s => s.ConvertToDictionaryAsync(mockSpmlDocument, ownerId))
                .ReturnsAsync(mockDictionary);
            _mockSpmlImportService.Setup(s => s.ConvertToSignsAsync(mockSpmlDocument, It.IsAny<int>()))
                .ReturnsAsync(mockSigns);

            // Act
            var result = await _controller.ImportSpmlFile(file, ownerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var importResult = Assert.IsType<SpmlImportResult>(okResult.Value);
            
            Assert.Equal(mockDictionary, importResult.Dictionary);
            Assert.Equal(mockSigns, importResult.Signs);
            Assert.Equal(4, importResult.OriginalPuddleId);
            Assert.Equal(1, importResult.TotalEntries);
            Assert.Equal(1, importResult.ValidSigns);
        }

        [Fact]
        public async Task ImportSpmlFile_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var file = CreateMockFile("test.spml", GetTestSpmlContent());
            
            _mockSpmlImportService.Setup(s => s.ParseSpmlAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Invalid SPML format"));

            // Act
            var result = await _controller.ImportSpmlFile(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Error importing SPML file", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task PreviewSpmlFile_WithNullFile_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.PreviewSpmlFile(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("No file provided", badRequestResult.Value);
        }

        [Fact]
        public async Task PreviewSpmlFile_WithValidFile_ShouldReturnPreview()
        {
            // Arrange
            var file = CreateMockFile("test.spml", GetTestSpmlContent());            var mockSpmlDocument = new SpmlDocument
            {
                PuddleId = 4,
                Terms = new List<string> { "Test Dictionary" },
                Type = "sgn",
                CreatedTimestamp = 1203374232,
                ModifiedTimestamp = 1311025253,
                Entries = new List<SpmlEntry>
                {                    new SpmlEntry
                    {
                        Id = 1,
                        Terms = new List<string> { "AS123", "test" },
                        TextElements = new List<string> { "test description" },
                        Video = "<iframe>test</iframe>",
                        Sources = new List<string> { "test source" },
                        User = "testuser",
                        CreatedTimestamp = 1311183542
                    },
                    new SpmlEntry
                    {
                        Id = 2,
                        Terms = new List<string> { "AS456", "hello" },
                        User = "testuser2",
                        CreatedTimestamp = 1311183600
                    }
                }
            };

            _mockSpmlImportService.Setup(s => s.ParseSpmlAsync(It.IsAny<string>()))
                .ReturnsAsync(mockSpmlDocument);

            // Act
            var result = await _controller.PreviewSpmlFile(file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var preview = Assert.IsType<SpmlPreview>(okResult.Value);

            Assert.Equal("Test Dictionary", preview.DictionaryName);
            Assert.Equal("sgn", preview.Type);
            Assert.Equal(4, preview.PuddleId);
            Assert.Equal(2, preview.TotalEntries);
            Assert.Equal(2, preview.EntriesWithGloss);
            Assert.Equal(1, preview.EntriesWithText);
            Assert.Equal(1, preview.EntriesWithVideo);
            Assert.Equal(1, preview.EntriesWithSource);
            Assert.Equal(2, preview.SampleEntries.Count);

            var firstSample = preview.SampleEntries.First();
            Assert.Equal(1, firstSample.Id);
            Assert.Equal("AS123", firstSample.FswNotation);
            Assert.Equal("test", firstSample.Gloss);
            Assert.Equal("testuser", firstSample.User);
        }

        [Fact]
        public async Task PreviewSpmlFile_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var file = CreateMockFile("test.spml", GetTestSpmlContent());
            
            _mockSpmlImportService.Setup(s => s.ParseSpmlAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Invalid SPML format"));

            // Act
            var result = await _controller.PreviewSpmlFile(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Error previewing SPML file", badRequestResult.Value?.ToString());
        }

        private static IFormFile CreateMockFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var file = new Mock<IFormFile>();
            
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(bytes));
            
            return file.Object;
        }

        private static string GetTestSpmlContent()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE spml SYSTEM ""http://www.signpuddle.net/spml_1.6.dtd"">
<spml root=""http://www.signbank.org/signpuddle1.6"" type=""sgn"" puddle=""4"" cdt=""1203374232"" mdt=""1311025253"" nextid=""11237"">
  <term><![CDATA[Test Dictionary]]></term>
  <entry id=""1"" cdt=""1311183542"" mdt=""1311183729"" usr=""test"">
    <term>AS17620S15a18S22a02M523x514S15a18478x487S22a02508x495S17620491x494</term>
    <term><![CDATA[test sign]]></term>
  </entry>
</spml>";
        }
    }
}
