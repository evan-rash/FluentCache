using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class CacheFailureTests
    {
        private class CacheMe
        {
            public CacheMe()
            {
                Random = new Random(0);
            }

            private readonly Random Random;

            public double DoWork()
            {
                return Random.NextDouble();
            }

            public async Task<double> DoWorkAsync()
            {
                await Task.Delay(TimeSpan.FromSeconds(.125));
                return DoWork();
            }
        }

        private Cache<CacheMe> CreateHandledCache(params CacheOperation[] failingOperations)
        {
            return new SimpleFailingCache(true, failingOperations).WithSource(new CacheMe());
        }

        private ICache<CacheMe> CreateUnhandledCache(params CacheOperation[] failingOperations)
        {
            return new SimpleFailingCache(false, failingOperations).WithSource(new CacheMe());
        }

        [TestMethod]
        public void CacheFailureOnGet_Handled()
        {
            ICache<CacheMe> cache = CreateHandledCache(CacheOperation.Get);

            double result = cache.Method(m => m.DoWork())
                                 .GetValue();

            Assert.AreNotEqual(default(double), result, "Even though there was an error, we should have handled it");
        }

        [TestMethod, ExpectedException(typeof(FluentCacheException))]
        public void CacheFailureOnGet_Unhandled()
        {
            ICache<CacheMe> cache = CreateUnhandledCache(CacheOperation.Get);

            double result = cache.Method(m => m.DoWork())
                                 .GetValue();

        }

        [TestMethod]
        public async Task CacheFailureOnGet_Async_Handled()
        {
            ICache<CacheMe> cache = CreateHandledCache(CacheOperation.Get);

            double result = await cache.Method(m => m.DoWorkAsync())
                                       .GetValueAsync();

            Assert.AreNotEqual(default(double), result, "Even though there was an error, we should have handled it");
        }

        [TestMethod, ExpectedException(typeof(FluentCacheException))]
        public async Task CacheFailureOnGet_Async_Unhandled()
        {
            ICache<CacheMe> cache = CreateUnhandledCache(CacheOperation.Get);

            double result = await cache.Method(m => m.DoWorkAsync())
                                       .GetValueAsync();

        }

        [TestMethod]
        public void CacheFailureOnSet_Handled()
        {
            ICache<CacheMe> cache = CreateHandledCache(CacheOperation.Set);

            CacheStrategy<double> cacheStrategy = cache.Method(m => m.DoWork());

            ICachedValue<double> result1 = cacheStrategy.Get();
            ICachedValue<double> result2 = cacheStrategy.Get();

            Assert.AreNotEqual(result1.Value, result2.Value);
        }

       [TestMethod, ExpectedException(typeof(FluentCacheException))]
        public void CacheFailureOnSet_Unhandled()
        {
            ICache<CacheMe> cache = CreateUnhandledCache(CacheOperation.Set);

            CacheStrategy<double> cacheStrategy = cache.Method(m => m.DoWork());

            ICachedValue<double> result = cacheStrategy.Get();
        }

       [TestMethod]
       public async Task CacheFailureOnSet_Async_Handled()
       {
           ICache<CacheMe> cache = CreateHandledCache(CacheOperation.Set);

           CacheStrategyAsync<double> cacheStrategy = cache.Method(m => m.DoWorkAsync());

           ICachedValue<double> result1 = await cacheStrategy.GetAsync();
           ICachedValue<double> result2 = await cacheStrategy.GetAsync();

           Assert.AreNotEqual(result1.Value, result2.Value);
       }

       [TestMethod, ExpectedException(typeof(FluentCacheException))]
       public async Task CacheFailureOnSet_Async_Unhandled()
       {
           ICache<CacheMe> cache = CreateUnhandledCache(CacheOperation.Set);

           CacheStrategyAsync<double> cacheStrategy = cache.Method(m => m.DoWorkAsync());

           ICachedValue<double> result = await cacheStrategy.GetAsync();
       }
    }
}
