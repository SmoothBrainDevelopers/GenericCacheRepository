using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Moq;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Helpers;
using Microsoft.Extensions.Caching.Memory;
using GenericCacheRepository.Interfaces;
using SqliteDbContext.Context;
using GenericCacheRepository.Tests.NUnit.Domain;
using GenericCacheRepository.Tests.NUnit.Context;

namespace GenericCacheRepository.Tests.NUnit.Tests
{
    public class CacheRepositoryTests_v1 : TestBase
    {
        [Test]
        public async Task FetchAsync_ReturnsCachedItem_WhenAvailable()
        {
            var alice = _dbContext.GenerateEntity<User>();
            alice.Name = "Alice";

            var result = await _repository.FetchAsync<User>(alice.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Alice", result.Name);
        }

        [Test]
        public async Task FetchAsync_FetchesFromDb_WhenNotInCache()
        {
            var bob = _dbContext.GenerateEntity<User>();
            bob.Name = "Bob";
            var result = await _repository.FetchAsync<User>(bob.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Bob", result.Name);
        }

        [Test]
        public async Task FetchAsync_UsesPaginationCorrectly()
        {
            var users = _dbContext.GenerateEntities<User>(2)
                .AsQueryable(); ;
            User alice = users.ElementAt(0);
            alice.Name = "Alice";

            User bob = users.ElementAt(1);
            bob.Name = "Bob";

            var query = new Query<User>(u => u.Id > 0);
            var result = await _repository.FetchAsync(1, 10, query);

            var expectedAlice = result.First(x => x.Id == alice.Id);

            Assert.AreEqual(alice.Id, expectedAlice.Id);
        }
    }
}