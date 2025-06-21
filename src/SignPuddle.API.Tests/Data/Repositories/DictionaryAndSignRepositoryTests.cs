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
    }
}
