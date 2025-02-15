using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Moq;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Helpers;
using Microsoft.Extensions.Caching.Memory;
using GenericCacheRepository.Interfaces;
using SqliteDbContext.Context;
using GenericCacheRepository.Test.MS.Domain;
using GenericCacheRepository.Test.MS.Context;

namespace GenericCacheRepository.Test.MS.Tests
{
    [TestClass]
    public class CacheRepositoryTests_v1 : TestBase
    {
        [TestMethod]
        public async Task FetchAsync_ReturnsCachedItem_WhenAvailable()
        {
            var alice = _dbContextMock.GenerateEntity<User>();
            alice.Name = "Alice";

            _cacheServiceMock.Setup(c => c.GetAsync<User>($"User:{alice.Id}")).ReturnsAsync(alice);

            var result = await _repository.FetchAsync<User>(alice.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Alice", result.Name);
        }

        [TestMethod]
        public async Task FetchAsync_FetchesFromDb_WhenNotInCache()
        {
            var bob = _dbContextMock.GenerateEntity<User>();
            bob.Name = "Bob";
            var result = await _repository.FetchAsync<User>(bob.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Bob", result.Name);
        }

        [TestMethod]
        public async Task FetchAsync_UsesPaginationCorrectly()
        {
            var users = _dbContextMock.GenerateEntities<User>(2)
                .AsQueryable(); ;
            User alice = users.ElementAt(0);
            alice.Name = "Alice";

            User bob = users.ElementAt(1);
            bob.Name = "Bob";

            var query = new Query<User>(u => u.Id > 0);
            var result = await _repository.FetchAsync(1, 10, query);

            Assert.AreEqual(2, result.Count);
        }
    }
}