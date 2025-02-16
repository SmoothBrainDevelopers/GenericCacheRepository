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
    public class CacheRepositoryTests : TestBase
    {
        [Test]
        public async Task DbContextProviderTest()
        {
            var user = _dbContext.GenerateEntities<User>(1).First();
            var origContext = _dbContext.Context;

            User entity1, entity2, entity3;

            using (var ctx = _dbContextProviderMock.Object.GetDbContext())
            {
                entity1 = ctx.Set<User>().Find(user.Id);
            }

            using (var ctx = _dbContextProviderMock.Object.GetDbContext())
            {
                entity2 = ctx.Set<User>().Find(user.Id);
            }

            entity3 = await _repository.FetchAsync(user.Id);

            Assert.IsNotNull(entity1);
            Assert.IsNotNull(entity2);
            Assert.IsNotNull(entity3);

            Assert.That(user.Name, Is.EqualTo(entity1.Name));
            Assert.That(user.Name, Is.EqualTo(entity2.Name));
            Assert.That(user.Name, Is.EqualTo(entity3.Name));
            Assert.That(entity1.UserName, Is.EqualTo(entity2.UserName));
            Assert.That(entity1.UserName, Is.EqualTo(entity3.UserName));
        }

        [Test]
        public async Task FetchAsync_ReturnsCachedItem_WhenAvailable()
        {
            var ctx = _dbContext.Context;
            var alice = _dbContext.GenerateEntity<User>();
            var fetchAfterGeneration = await _repository.FetchAsync(alice.Id); //user is cached

            Assert.IsNotNull(fetchAfterGeneration);
            Assert.AreEqual(alice.Name, fetchAfterGeneration.Name);
            
            var instance = ctx.Users.FirstOrDefault(x => x.Id == alice.Id); //bypass cache to get tracked object
            alice.Name = "Alice";
            Assert.AreEqual(alice.Name, instance.Name);

            fetchAfterGeneration = await _repository.FetchAsync(alice.Id); //user is cached, but object is tracked
            Assert.AreNotEqual(alice.Name, fetchAfterGeneration.Name);

            await _repository.SaveAsync(alice); //update instance of tracked object and cached user
            var fetchedAfterUpdate = await _repository.FetchAsync(alice.Id);
            
            Assert.IsNotNull(fetchedAfterUpdate);
            Assert.AreEqual(alice.Name, fetchedAfterUpdate.Name);
        }

        [Test]
        public async Task FetchAsync_FetchesFromDb_WhenNotInCache()
        {
            var ctx = _dbContext.Context;
            var bob = _dbContext.GenerateEntity<User>();
            bob.Name = "Bob";
            ctx.SaveChanges();

            var result = await _repository.FetchAsync(bob.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(bob.Name, result.Name);
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