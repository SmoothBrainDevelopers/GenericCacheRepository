namespace GenericCacheRepository.Test.MS.Tests
{
    [TestClass]
    public class CacheRepositoryTests_v2 : TestBase
    {
        [TestMethod]
        public async Task FetchAsync_ReturnsCachedItem_WhenAvailable()
        {
            var user = _dbContextMock.GenerateEntity<User>();
            user.Name = "Alice";
            _cacheServiceMock.Setup(c => c.GetAsync<User>("User:1")).ReturnsAsync(user);

            var result = await _repository.FetchAsync<User>(1);

            Assert.IsNotNull(result);
            Assert.AreEqual("Alice", result.Name);
        }

        [TestMethod]
        public async Task FetchAsync_UsesPaginationCorrectly()
        {
            var users = _dbContextMock.GenerateEntities<User>(2);
            users[0].UserName = "Alice";
            users[1].UserName = "Bob";

            var query = new Query<User>(u => u.UserId > 0, sortByIndex: 1, ascending: false);
            var result = await _repository.FetchAsync(1, 10, query);

            Assert.AreEqual(2, result.Count);
        }
    }
}
