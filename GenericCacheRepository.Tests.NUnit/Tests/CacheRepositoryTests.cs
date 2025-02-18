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

            entity3 = await _userCacheRepository.FetchAsync(user.Id);

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
            var generatedAlice = _dbContext.GenerateEntity<User>(user => { user.Name = "Alice"; }); //generated in DB
            var fetchedAlice = await _userCacheRepository.FetchAsync(generatedAlice.Id); //user is cached

            Assert.IsNotNull(fetchedAlice);
            Assert.AreEqual(generatedAlice.Name, fetchedAlice.Name);

            var bypassAlice = ctx.Users.First(x => x.Id == generatedAlice.Id); //bypass cache to get tracked object
            bypassAlice.Name = "Bob";
            var bob = bypassAlice;
            ctx.SaveChanges(); //changes are saved to DB, but not cache
            fetchedAlice = await _userCacheRepository.FetchAsync(generatedAlice.Id); //user is cached, but object is tracked
            Assert.AreNotEqual(bypassAlice.Name, fetchedAlice.Name);
            Assert.AreEqual(bypassAlice.Name, bob.Name);

            fetchedAlice.Name = "Alice";
            await _userCacheRepository.SaveAsync(fetchedAlice); //update instance of tracked object and cached user
            var fetchedAliceAfterUpdate = await _userCacheRepository.FetchAsync(fetchedAlice.Id);
            
            bypassAlice = ctx.Users.First(x => x.Id == generatedAlice.Id); //bypass cache to get tracked object
            Assert.IsNotNull(fetchedAliceAfterUpdate);
            Assert.AreNotEqual(bypassAlice.Name, fetchedAliceAfterUpdate.Name);
        }

        [Test]
        public async Task FetchAsync_FetchesFromDb_WhenNotInCache()
        {
            var ctx = _dbContext.Context;
            var bob = _dbContext.GenerateEntity<User>();
            bob.Name = "Bob";
            ctx.SaveChanges();

            var result = await _userCacheRepository.FetchAsync(bob.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(bob.Name, result.Name);
        }

        [Test]
        public async Task FetchAsync_UsesPaginationCorrectly()
        {
            var users = _dbContext.GenerateEntities<User>(20) //generated in DB
                .AsQueryable(); ;
            User alice = users.OrderBy(x => x.Name).ElementAt(0);
            alice.Name = "Alice";

            var result = await _userCacheRepository.FetchPageAsync(1, 10, nameof(User.Name));
            var expectedAlice = result.FirstOrDefault(x => x.Id == alice.Id);
            Assert.AreNotEqual(alice.Name, expectedAlice?.Name);

            await _userCacheRepository.SaveAsync(alice); //save changes and update cache
            result = await _userCacheRepository.FetchPageAsync(1, 10, nameof(User.Name));
            expectedAlice = result.First(x => x.Id == alice.Id);
            Assert.AreEqual(alice.Name, expectedAlice.Name);
        }
    }
}