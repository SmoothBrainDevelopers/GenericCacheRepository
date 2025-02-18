using GenericCacheRepository.Tests.NUnit.Domain;

namespace GenericCacheRepository.Tests.NUnit.Tests
{
    public class CompositeKeyCacheRepositoryTests : TestBase
    {
        [Test]
        public async Task FetchCompositeKey_ShouldReturnCachedPurchase()
        {
            var purchase = _dbContext.GenerateEntity<Purchase>();
            string compositeKey = $"Purchase:{purchase.CustomerId}:{purchase.StoreId}:{purchase.ProductId}:{purchase.PurchaseDate:yyyy-MM-dd}";

            await _cacheService.SetAsync(compositeKey, purchase, TimeSpan.FromMinutes(10));
            var cachedPurchase = await _cacheService.GetAsync<Purchase>(compositeKey);

            Assert.IsNotNull(cachedPurchase);
            Assert.AreEqual(purchase.CustomerId, cachedPurchase.CustomerId);
            Assert.AreEqual(purchase.StoreId, cachedPurchase.StoreId);
            Assert.AreEqual(purchase.ProductId, cachedPurchase.ProductId);
        }

        [Test]
        public async Task FetchCompositeKey_ShouldReturnFromDBIfNotCached()
        {
            var purchase = _dbContext.GenerateEntity<Purchase>();
            string compositeKey = $"Purchase:{purchase.CustomerId}:{purchase.StoreId}:{purchase.ProductId}:{purchase.PurchaseDate:yyyy-MM-dd}";

            var fetchedPurchase = await _purchaseCacheRepository.FetchAsync(purchase.CustomerId, purchase.StoreId, purchase.ProductId, purchase.PurchaseDate);

            Assert.IsNotNull(fetchedPurchase);
            Assert.AreEqual(purchase.CustomerId, fetchedPurchase.CustomerId);
            Assert.AreEqual(purchase.StoreId, fetchedPurchase.StoreId);
            Assert.AreEqual(purchase.ProductId, fetchedPurchase.ProductId);
        }

        [Test]
        public async Task StoreCompositeKey_ShouldSaveToCache()
        {
            var purchase = _dbContext.GenerateEntity<Purchase>();
            string compositeKey = $"Purchase:{purchase.CustomerId}:{purchase.StoreId}:{purchase.ProductId}:{purchase.PurchaseDate:yyyy-MM-dd}";

            await _cacheService.SetAsync(compositeKey, purchase, TimeSpan.FromMinutes(10));
            var cachedPurchase = await _cacheService.GetAsync<Purchase>(compositeKey);

            Assert.IsNotNull(cachedPurchase);
            Assert.AreEqual(purchase.CustomerId, cachedPurchase.CustomerId);
        }

        [Test]
        public async Task RemoveCompositeKey_ShouldDeleteFromCache()
        {
            var purchase = _dbContext.GenerateEntity<Purchase>();
            string compositeKey = $"Purchase:{purchase.CustomerId}:{purchase.StoreId}:{purchase.ProductId}:{purchase.PurchaseDate:yyyy-MM-dd}";

            await _cacheService.SetAsync(compositeKey, purchase, TimeSpan.FromMinutes(10));
            await _cacheService.RemoveAsync(compositeKey);
            var cachedPurchase = await _cacheService.GetAsync<Purchase>(compositeKey);

            Assert.IsNull(cachedPurchase);
        }

        [Test]
        public async Task FetchMultipleCompositeKeys_ShouldReturnCorrectResults()
        {
            var purchase1 = _dbContext.GenerateEntity<Purchase>();
            var purchase2 = _dbContext.GenerateEntity<Purchase>();

            string compositeKey1 = $"Purchase:{purchase1.CustomerId}:{purchase1.StoreId}:{purchase1.ProductId}:{purchase1.PurchaseDate:yyyy-MM-dd}";
            string compositeKey2 = $"Purchase:{purchase2.CustomerId}:{purchase2.StoreId}:{purchase2.ProductId}:{purchase2.PurchaseDate:yyyy-MM-dd}";

            await _cacheService.SetAsync(compositeKey1, purchase1, TimeSpan.FromMinutes(10));
            await _cacheService.SetAsync(compositeKey2, purchase2, TimeSpan.FromMinutes(10));

            var cachedPurchase1 = await _cacheService.GetAsync<Purchase>(compositeKey1);
            var cachedPurchase2 = await _cacheService.GetAsync<Purchase>(compositeKey2);

            Assert.IsNotNull(cachedPurchase1);
            Assert.IsNotNull(cachedPurchase2);
        }
    }

}
