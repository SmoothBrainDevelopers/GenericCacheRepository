using GenericCacheRepository.Helpers;
using GenericCacheRepository.Tests.NUnit.Domain;

namespace GenericCacheRepository.Tests.NUnit.Tests
{
    public class CacheRepositoryTests_v2 : TestBase
    {
        [Test]
        public async Task FetchAsync_ReturnsCachedItem_WhenAvailable()
        {
            var user = _dbContext.GenerateEntity<User>();
            user.Name = "Alice";
            var result = await _repository.FetchAsync<User>(user.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Alice", result.Name);
        }

        [Test]
        public async Task FetchAsync_UsesPaginationCorrectly()
        {
            var users = _dbContext.GenerateEntities<User>(2);
            users[0].UserName = "Alice";
            users[1].UserName = "Bob";

            var query = new Query<User>(u => u.UserId > 0, sortByIndex: 1, ascending: false);
            var result = await _repository.FetchAsync(1, 10, query);
            var expectedAlice = result.First(x => x.UserName == "Alice");

            Assert.AreEqual(users[0].Id, expectedAlice.Id);
        }
    }
}
