using GenericCacheRepository.Interfaces;
using GenericCacheRepository.Repository;
using GenericCacheRepository.Services;
using GenericCacheRepository.Test.MS.Context;
using Moq;
using SqliteDbContext.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Test.MS.Tests
{
    public class TestBase
    {
        protected SqliteDbContext<TestDbContext> _dbContextMock;
        protected Mock<ICacheService> _cacheServiceMock;
        protected Mock<ICompositeCacheService> _compositeCacheServiceMock;
        protected CacheRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _dbContextMock = new SqliteDbContext<TestDbContext>();
            _cacheServiceMock = new Mock<ICacheService>();
            _compositeCacheServiceMock = new Mock<ICompositeCacheService>();
            _repository = new CacheRepository(_cacheServiceMock.Object, _compositeCacheServiceMock.Object, _dbContextMock.Context);
            RegisterTypes();
        }

        private static HashSet<Type> _registeredTypes = new HashSet<Type>();
        private static void RegisterTypes()
        {
            RegisterType<User>(() =>
            {
                SqliteDbContext<TestDbContext>.RegisterKeyAssignment<User>((user, seeder) =>
                {
                    user.Id = (int)seeder.IncrementKeys<User>().First();
                });
            });
            
        }

        private static void RegisterType<T>(Action action)
        {
            if (_registeredTypes.Contains(typeof(T)))
                return;
            _registeredTypes.Add(typeof(T));
            action();
        }
    }
}
