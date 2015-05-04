using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using FluentCache;
using System.Collections.Generic;
using System.Net.Http;

namespace FluentCache.Test
{
    [TestClass]
    public class CacheStrategyTests
    {
        private ICache<CacheStrategyTests> CreateCache()
        {
            return new SimpleCache().WithSource(this);
        }

        private double CalculateSomeWork()
        {
            return Math.Sqrt(1234);
        }

        private double CalculateSomeWork(int power)
        {
            return Math.Pow(Math.E, power);
        }

        private double CalculateSomeWork(int num, string text)
        {
            return Math.Pow(Math.E, num) + text.GetHashCode();
        }

        private async Task<string> DownloadTextAsync(string url)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        } 

        [TestMethod]
        public void TestMethodGet()
        {
            ICache cache = CreateCache();

            ICachedValue<double> result = cache.ThisMethod()
                                               .Get<double>();

            Assert.IsNull(result, "Should return null since we didn't specify a retrieval mechanism");

        }

        [TestMethod]
        public void TestMethodGetValue()
        {
            ICache cache = CreateCache();

            double result = cache.ThisMethod()
                                 .GetValue<double>();

            Assert.AreEqual(default(double), result, "Should return default since we didn't specify a retrieval mechanism");

        }

        [TestMethod]
        public void TestMethodRetrievalGet()
        {
            ICache cache = CreateCache();

            ICachedValue<double> result = cache.ThisMethod()
                                               .RetrieveUsing(CalculateSomeWork)
                                               .Get();

            Assert.IsNotNull(result, "Should not return null since we specified a retrieval mechanism");
            Assert.AreEqual(1, ((SimpleCachedValueWrapper<double>)result).CachedValue.AccessCount, "Should only have accessed this value once");
        }

        [TestMethod]
        public void TestMethodRetrievalGetValue()
        {
            ICache cache = CreateCache();

            double result = cache.ThisMethod()
                                 .RetrieveUsing(CalculateSomeWork)
                                 .GetValue();

            Assert.AreNotEqual(default(double), "Should not return 0 since we specified a retrieval mechanism");
        }

        [TestMethod]
        public void TestMethodRetrievalGetMultiple()
        {
            ICache cache = CreateCache();

            CacheStrategy<double> cacheStrategy = cache.ThisMethod()
                                                       .RetrieveUsing(CalculateSomeWork);

            for (int i = 0; i < 10; i++)
            {
                var result = cacheStrategy.Get() as SimpleCachedValueWrapper<double>;
                Assert.AreEqual(i + 1, result.CachedValue.AccessCount, "Expected run # {0} to have access count {0}", i + 1);
            }
        }

        [TestMethod]
        public void TestInvalidation()
        {
            ICache cache = CreateCache();

            Func<ICachedValue<double>, bool> invalidateIf = existing =>
                {
                    SimpleCachedValueWrapper<double> wrapper = existing as SimpleCachedValueWrapper<double>;
                    return wrapper != null && wrapper.CachedValue.AccessCount > 2;
                };

            CacheStrategy<double> strategy = cache.ThisMethod()
                                                  .RetrieveUsing(CalculateSomeWork)
                                                  .InvalidateIf(invalidateIf);

            var result1 = strategy.Get() as SimpleCachedValueWrapper<double>;
            var result2 = strategy.Get() as SimpleCachedValueWrapper<double>;
            var result3 = strategy.Get() as SimpleCachedValueWrapper<double>;

            Assert.AreEqual(1, result3.CachedValue.AccessCount, "The 3rd execution should have been invalidated");
        }

        [TestMethod]
        public async Task TestExpireOn()
        {
            ICache cache = CreateCache();

            DateTime now = DateTime.UtcNow;
            TimeSpan wait = TimeSpan.FromSeconds(.5);
            DateTime expireAt = now.Add(wait);

            CacheStrategy<double> strategy = cache.ThisMethod()
                                                  .RetrieveUsing(CalculateSomeWork)
                                                  .ExpireOn(expireAt);

            var result1 = strategy.Get() as SimpleCachedValueWrapper<double>;

            await Task.Delay(wait);

            var result2 = strategy.Get() as SimpleCachedValueWrapper<double>;

            Assert.AreEqual(1, result1.CachedValue.AccessCount);
            Assert.AreEqual(1, result2.CachedValue.AccessCount, "The 2nd call should have expired");
        }

        [TestMethod]
        public async Task TestExpireAfter()
        {
            ICache cache = CreateCache();

            TimeSpan wait = TimeSpan.FromSeconds(.5);

            CacheStrategy<double> strategy = cache.ThisMethod()
                                                  .RetrieveUsing(CalculateSomeWork)
                                                  .ExpireAfter(wait);

            strategy.Get();
            for (int i = 1; i < 10; i++)
            {
                var result = strategy.Get() as SimpleCachedValueWrapper<double>;
                Assert.AreNotEqual(1, result.CachedValue.AccessCount, "These calls should be cached");

                await Task.Delay(TimeSpan.FromSeconds(wait.TotalSeconds/2.0));
            }

            await Task.Delay(wait);

            var expiredResult = strategy.Get() as SimpleCachedValueWrapper<double>;

            Assert.AreEqual(1, expiredResult.CachedValue.AccessCount, "This call should have expired after waiting for {0}", wait);
        }

        [TestMethod]
        public void TestParameterized1()
        {
            ICache cache = CreateCache();

            double result = cache.ThisMethod()
                                 .WithParameters(3)
                                 .RetrieveUsing(CalculateSomeWork)
                                 .GetValue();

            Assert.AreNotEqual(0, result, "This should be a calculated value");
        }

        [TestMethod]
        public void TestParameterized2()
        {
            ICache cache = CreateCache();

            double result = cache.ThisMethod()
                                 .WithParameters(3, "hello")
                                 .Retrieve(CalculateSomeWork)
                                 .GetValue();

            Assert.AreNotEqual(0, result, "This should be a calculated value");
        }

        [TestMethod]
        public async Task TestGetAsync()
        {
            var cache = CreateCache();

            CacheStrategyAsync<string> strategy = cache.ThisMethod()
                                                       .WithParameters("http://www.google.com")
                                                       .RetrieveUsingAsync(DownloadTextAsync);

            SimpleCachedValueWrapper<string> result1 = await strategy.GetAsync() as SimpleCachedValueWrapper<string>;
            SimpleCachedValueWrapper<string> result2 = await strategy.GetAsync() as SimpleCachedValueWrapper<string>;

            Assert.AreEqual(2, result2.CachedValue.AccessCount);
        }

        [TestMethod]
        public async Task TestGetValueAsync()
        {
            var cache = CreateCache();
            
            string text = await cache.ThisMethod()
                                     .WithParameters("http://www.google.com")
                                     .RetrieveUsingAsync(DownloadTextAsync)
                                     .GetValueAsync();

            Assert.IsNotNull(text);
        }

        [TestMethod]
        public async Task TestGetValueAsyncLambda()
        {
            var Cache = CreateCache();

            string html = await Cache.ThisMethod()
                                     .WithParameters("http://www.google.com")
                                     .RetrieveUsingAsync(async (url) => await Http.GetStringAsync(url))
                                     .ExpireAfter(TimeSpan.FromMinutes(30))
                                     .GetValueAsync();
        }



        

        private static class Http
        {
            public static async Task<string> GetStringAsync(string url)
            {
                using (var client = new HttpClient())
                {
                    return await client.GetStringAsync(url);
                }
            }
        }

    }
}
