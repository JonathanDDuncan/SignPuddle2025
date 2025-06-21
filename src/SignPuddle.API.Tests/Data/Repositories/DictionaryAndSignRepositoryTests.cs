using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Data;
using SignPuddle.API.Data.Repositories;
using SignPuddle.API.Models;
using Xunit;

namespace SignPuddle.API.Tests.Data.Repositories
{
    public class DictionaryAndSignRepositoryTests
    {
        private ApplicationDbContext GetDbContext([System.Runtime.CompilerServices.CallerMemberName] string dbName = "TestDb")
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName + System.Guid.NewGuid())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void BuildSearchQuery_Dictionary_Works()
        {
            var context = GetDbContext();
            context.Dictionaries.AddRange(new List<Dictionary>
            {
                new Dictionary { Id = "1", Name = "Animals", Description = "Animal signs", IsPublic = true, OwnerId = "user1" },
                new Dictionary { Id = "2", Name = "Colors", Description = "Color signs", IsPublic = false, OwnerId = "user2" },
                new Dictionary { Id = "3", Name = "Fruits", Description = "Fruit signs", IsPublic = true, OwnerId = "user1" }
            });
            context.SaveChanges();
            var repo = new DictionaryRepository(context);
            var parameters = new DictionarySearchParameters { Query = "Fruit", OwnerId = "user1", IsPublic = true };
            var query = repo.BuildSearchQuery(parameters);
            var results = query.ToList();
            Assert.Single(results);
            Assert.Equal("Fruits", results[0].Name);
        }

        [Fact]
        public void BuildSearchQuery_Sign_Works()
        {
            var context = GetDbContext();
            context.Signs.AddRange(new List<Sign>
            {
                new Sign { PuddleSignId = 1, Gloss = "cat", DictionaryId = "1", Fsw = "M100x100S2" },
                new Sign { PuddleSignId = 2, Gloss = "dog", DictionaryId = "1", Fsw = "M200x200S3" },
                new Sign { PuddleSignId = 3, Gloss = "apple", DictionaryId = "2", Fsw = "M300x300S4" }
            });
            context.SaveChanges();
            var repo = new SignRepository(context);
            var parameters = new SignSearchParameters { Gloss = "cat", DictionaryId = "1" };
            var query = repo.BuildSearchQuery(parameters);
            var results = query.ToList();
            Assert.Single(results);
            Assert.Equal("cat", results[0].Gloss);
        }

        [Fact]
        public async Task ExecuteSearchQueryAsync_Dictionary_Pagination_Works()
        {
            var context = GetDbContext();
            context.Dictionaries.AddRange(new List<Dictionary>
            {
                new Dictionary { Id = "1", Name = "Animals", Description = "Animal signs", IsPublic = true, OwnerId = "user1" },
                new Dictionary { Id = "2", Name = "Colors", Description = "Color signs", IsPublic = false, OwnerId = "user2" },
                new Dictionary { Id = "3", Name = "Fruits", Description = "Fruit signs", IsPublic = true, OwnerId = "user1" }
            });
            context.SaveChanges();
            var repo = new DictionaryRepository(context);
            var parameters = new DictionarySearchParameters { };
            var query = repo.BuildSearchQuery(parameters);
            var results = await repo.ExecuteSearchQueryAsync(query, page: 2, pageSize: 2);
            Assert.Single(results);
            Assert.Equal("Fruits", results[0].Name);
        }

        [Fact]
        public async Task ExecuteSearchQueryAsync_Sign_Pagination_Works()
        {
            var context = GetDbContext();
            context.Signs.AddRange(new List<Sign>
            {
                new Sign { PuddleSignId = 1, Gloss = "cat", DictionaryId = "1", Fsw = "M100x100S2" },
                new Sign { PuddleSignId = 2, Gloss = "dog", DictionaryId = "1", Fsw = "M200x200S3" },
                new Sign { PuddleSignId = 3, Gloss = "apple", DictionaryId = "2", Fsw = "M300x300S4" }
            });
            context.SaveChanges();
            var repo = new SignRepository(context);
            var parameters = new SignSearchParameters { };
            var query = repo.BuildSearchQuery(parameters);
            var results = await repo.ExecuteSearchQueryAsync(query, page: 2, pageSize: 2);
            Assert.Single(results);
            Assert.Equal("apple", results[0].Gloss);
        }

        [Fact]
        public void MapToDto_Dictionary_Works()
        {
            var entity = new Dictionary { Id = "1", Name = "Test", Description = "Desc", IsPublic = true, OwnerId = "user1" };
            var dto = DictionaryRepository.MapToDto(entity);
            Assert.Equal("1", dto.Id);
            Assert.Equal("Test", dto.Name);
            Assert.Equal("Desc", dto.Description);
            Assert.True(dto.IsPublic);
            Assert.Equal("user1", dto.OwnerId);
        }

        [Fact]
        public void MapToDto_Sign_Works()
        {
            var entity = new Sign { PuddleSignId = 5, Gloss = "cat", DictionaryId = "1", Fsw = "M100x100S2" };
            var dto = SignRepository.MapToDto(entity);
            Assert.Equal(5, dto.PuddleSignId);
            Assert.Equal("cat", dto.Gloss);
            Assert.Equal("1", dto.DictionaryId);
            Assert.Equal("M100x100S2", dto.Fsw);
        }

        [Fact]
        public void Validate_DictionarySearchParameters_Works()
        {
            var p = new DictionarySearchParameters { Page = 0, PageSize = 200, Query = "  test  ", OwnerId = "  user  " };
            p.Validate();
            Assert.Equal(1, p.Page);
            Assert.Equal(10, p.PageSize);
            Assert.Equal("test", p.Query);
            Assert.Equal("user", p.OwnerId);
        }

        [Fact]
        public void Validate_SignSearchParameters_Works()
        {
            var p = new SignSearchParameters { Page = -5, PageSize = 0, Gloss = "  cat  ", DictionaryId = " 1 ", Fsw = "  fsw  " };
            p.Validate();
            Assert.Equal(1, p.Page);
            Assert.Equal(10, p.PageSize);
            Assert.Equal("cat", p.Gloss);
            Assert.Equal("1", p.DictionaryId);
            Assert.Equal("fsw", p.Fsw);
        }

        [Fact]
        public async Task CountSearchResultsAsync_Dictionary_Works()
        {
            var context = GetDbContext();
            context.Dictionaries.AddRange(new List<Dictionary>
            {
                new Dictionary { Id = "1", Name = "Animals", Description = "Animal signs", IsPublic = true, OwnerId = "user1" },
                new Dictionary { Id = "2", Name = "Colors", Description = "Color signs", IsPublic = false, OwnerId = "user2" },
                new Dictionary { Id = "3", Name = "Fruits", Description = "Fruit signs", IsPublic = true, OwnerId = "user1" }
            });
            context.SaveChanges();
            var repo = new DictionaryRepository(context);
            var parameters = new DictionarySearchParameters { IsPublic = true };
            var query = repo.BuildSearchQuery(parameters);
            var count = await repo.CountSearchResultsAsync(query);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task CountSearchResultsAsync_Sign_Works()
        {
            var context = GetDbContext();
            context.Signs.AddRange(new List<Sign>
            {
                new Sign { PuddleSignId = 1, Gloss = "cat", DictionaryId = "1", Fsw = "M100x100S2" },
                new Sign { PuddleSignId = 2, Gloss = "dog", DictionaryId = "1", Fsw = "M200x200S3" },
                new Sign { PuddleSignId = 3, Gloss = "apple", DictionaryId = "2", Fsw = "M300x300S4" }
            });
            context.SaveChanges();
            var repo = new SignRepository(context);
            var parameters = new SignSearchParameters { DictionaryId = "1" };
            var query = repo.BuildSearchQuery(parameters);
            var count = await repo.CountSearchResultsAsync(query);
            Assert.Equal(2, count);
        }
    }
}
