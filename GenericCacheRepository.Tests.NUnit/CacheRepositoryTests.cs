using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GenericCacheRepositoryTests
{
    [TestClass]
    public class CacheRepositoryTests
    {
        private Mock<DbContext> _dbContextMock;
        private Mock<CacheService> _cacheServiceMock;
        private CacheRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _dbContextMock = new Mock<DbContext>();
            _cacheServiceMock = new Mock<CacheService>();
            _repository = new CacheRepository(_cacheServiceMock.Object, _dbContextMock.Object);
        }

        [TestMethod]
        public async Task FetchAsync_ReturnsCachedItem_WhenAvailable()
        {
            var user = new User { Id = 1, Name = "Alice" };
            _cacheServiceMock.Setup(c => c.GetAsync<User>("User:1")).ReturnsAsync(user);

            var result = await _repository.FetchAsync<User>(1);

            Assert.IsNotNull(result);
            Assert.AreEqual("Alice", result.Name);
        }

        [TestMethod]
        public async Task FetchAsync_FetchesFromDb_WhenNotInCache()
        {
            var user = new User { Id = 2, Name = "Bob" };
            var dbSetMock = new Mock<DbSet<User>>();
            dbSetMock.Setup(d => d.FindAsync(2)).ReturnsAsync(user);
            _dbContextMock.Setup(d => d.Set<User>()).Returns(dbSetMock.Object);

            var result = await _repository.FetchAsync<User>(2);

            Assert.IsNotNull(result);
            Assert.AreEqual("Bob", result.Name);
        }

        [TestMethod]
        public async Task FetchAsync_UsesPaginationCorrectly()
        {
            var users = new List<User>
        {
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" }
        }.AsQueryable();

            var dbSetMock = new Mock<DbSet<User>>();
            dbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            dbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            dbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            dbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _dbContextMock.Setup(d => d.Set<User>()).Returns(dbSetMock.Object);

            var query = new Query<User>(u => u.Id > 0);
            var result = await _repository.FetchAsync(1, 10, query);

            Assert.AreEqual(2, result.Count);
        }
    }

}