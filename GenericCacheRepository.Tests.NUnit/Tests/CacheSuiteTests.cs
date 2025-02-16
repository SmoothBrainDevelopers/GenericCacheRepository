using GenericCacheRepository.Helpers;
using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Tests.NUnit.Context;
using GenericCacheRepository.Tests.NUnit.Domain;
using Moq;

namespace GenericCacheRepository.Tests.NUnit.Tests
{
    class CacheSuiteTests
    {
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<ICompositeCacheService> _compositeCacheServiceMock;
        private Mock<ICacheRepository<User>> _cacheRepositoryMock;
        private Mock<ILoggerService> _loggerMock;
        private CircuitBreaker _circuitBreaker;

        [SetUp]
        public void Setup()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _compositeCacheServiceMock = new Mock<ICompositeCacheService>();
            _cacheRepositoryMock = new Mock<ICacheRepository<User>>();
            _loggerMock = new Mock<ILoggerService>();
            _circuitBreaker = new CircuitBreaker();
            _circuitBreaker.SetTimeout(60);
        }

        // Cache Service Tests
        [Test]
        public async Task CacheService_ShouldRetrieveCachedItem()
        {
            var key = "User:1";
            var user = new { Id = 1, Name = "Alice" };
            _cacheServiceMock.Setup(c => c.GetAsync<object>(key)).ReturnsAsync(user);

            var cachedUser = await _cacheServiceMock.Object.GetAsync<object>(key);

            Assert.IsNotNull(cachedUser);
            Assert.AreEqual(1, ((dynamic)cachedUser).Id);
        }

        [Test]
        public async Task CacheService_ShouldStoreItemInCache()
        {
            var key = "User:2";
            var user = new { Id = 2, Name = "Bob" };
            _cacheServiceMock.Setup(c => c.SetAsync(key, user, TimeSpan.FromMinutes(10))).Returns(Task.CompletedTask);

            await _cacheServiceMock.Object.SetAsync(key, user, TimeSpan.FromMinutes(10));

            _cacheServiceMock.Verify(c => c.SetAsync(key, user, TimeSpan.FromMinutes(10)), Times.Once);
        }

        // Cache Repository Tests
        [Test]
        public async Task CacheRepository_ShouldFetchFromCacheBeforeDB()
        {
            var key = "User:3";
            var user = new User() { Id = 3, Name = "Charlie" };
            _cacheServiceMock.Setup(c => c.GetAsync<object>(key)).ReturnsAsync(user);
            _cacheRepositoryMock.Setup(r => r.FetchAsync(3)).ReturnsAsync(user);

            var result = await _cacheRepositoryMock.Object.FetchAsync(3);

            Assert.IsNotNull(result);
            Assert.AreEqual("Charlie", ((dynamic)result).Name);
        }

        // Circuit Breaker Tests
        [Test]
        public void CircuitBreaker_ShouldBlockAndRecovertAfterCooldown()
        {
            _circuitBreaker.SetTimeout(1);
            _circuitBreaker.RecordFailure();
            _circuitBreaker.RecordFailure();
            _circuitBreaker.RecordFailure();

            Assert.IsFalse(_circuitBreaker.AllowRequest());
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1)); // Simulate cooldown
            Assert.IsTrue(_circuitBreaker.AllowRequest());
        }

        // Composite Key Service Tests
        [Test]
        public void CompositeCacheService_ShouldStoreAndRetrieveCompositeKeys()
        {
            var compositeKey = "Query:User:Active";
            var userIds = new List<object> { 1, 2, 3 };
            _compositeCacheServiceMock.Setup(c => c.SetCachedIds(compositeKey, userIds, TimeSpan.FromMinutes(10)));
            _compositeCacheServiceMock.Setup(c => c.GetCachedIds(compositeKey)).Returns(userIds);

            _compositeCacheServiceMock.Object.SetCachedIds(compositeKey, userIds, TimeSpan.FromMinutes(10));
            var cachedIds = _compositeCacheServiceMock.Object.GetCachedIds(compositeKey);

            Assert.AreEqual(3, cachedIds.Count);
        }
    }
}