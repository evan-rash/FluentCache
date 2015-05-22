using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class RedisTests
    {
        private static StackExchange.Redis.ConnectionMultiplexer Redis;

        [ClassInitialize]
        public static void InitializeRedis(TestContext context)
        {
            var config = StackExchange.Redis.ConfigurationOptions.Parse("localhost");
            config.AllowAdmin = true;

            Redis = StackExchange.Redis.ConnectionMultiplexer.Connect(config);

        }

        [TestInitialize]
        public void TestInitialize()
        {
            var endpoint = Redis.GetEndPoints().Single();
            Redis.GetServer(endpoint).FlushAllDatabases();
        }

        public class CacheMe
        {
            public double DoSomeWork()
            {
                return Math.Pow(1000, 1000);
            }

            public double DoSomeWorkParameterized(int parameter)
            {
                return Math.Exp(parameter);
            }
        }

        private Cache<CacheMe> CreateCache()
        {
            return new Redis.FluentRedisCache(Redis).WithSource(new CacheMe());
        }

        [TestMethod]
        public void Redis_SimpleCache()
        {
            var cache = CreateCache();
            var cacheStrategy = cache.Method(c => c.DoSomeWork());

            CachedValue<double> firstResult = cacheStrategy.Get();
            Assert.AreEqual(0L, firstResult.Version);

            CachedValue<double> secondResult = cacheStrategy.Get();
            Assert.AreEqual(0L, secondResult.Version);
        }

        [TestMethod]
        public void Redis_SimpleCache_Parameter()
        {
            var cache = CreateCache();

            var cacheStrategy1 = cache.Method(c => c.DoSomeWorkParameterized(1));
            CachedValue<double> firstResult = cacheStrategy1.Get();
            Assert.AreEqual(0L, firstResult.Version);

            var cacheStrategy2 = cache.Method(c => c.DoSomeWorkParameterized(2));
            CachedValue<double> secondResult = cacheStrategy2.Get();
            Assert.AreEqual(0L, secondResult.Version);

            Assert.AreNotEqual(firstResult.Value, secondResult.Value);
        }

        [TestMethod]
        public async Task Redis_ExpireAfter()
        {
            var cache = CreateCache();
            var cacheStrategy = cache.Method(c => c.DoSomeWork())
                                     .ExpireAfter(TimeSpan.FromSeconds(3));

            var start = DateTime.UtcNow;

            CachedValue<double> firstResult = cacheStrategy.Get();
            Assert.AreEqual(0L, firstResult.Version);

            await Task.Delay(TimeSpan.FromSeconds(2));

            //we should still get the same cached version of data
            CachedValue<double> secondResult = cacheStrategy.Get();
            Assert.AreEqual(firstResult.CachedDate, secondResult.CachedDate);

            await Task.Delay(TimeSpan.FromSeconds(4));

            //we should have a new version of data because the sliding expiration has expired
            CachedValue<double> thirdResult = cacheStrategy.Get();

            Assert.AreNotEqual(firstResult.CachedDate, thirdResult.CachedDate);
        }

        [TestMethod]
        public async Task Redis_ClearCache()
        {
            var cache = CreateCache();
            var cacheStrategy = cache.Method(c => c.DoSomeWork());

            CachedValue<double> firstResult = cacheStrategy.Get();
            Assert.AreEqual(0L, firstResult.Version);

            cacheStrategy.ClearValue();

            await Task.Delay(TimeSpan.FromSeconds(.125));

            CachedValue<double> secondResult = cacheStrategy.Get();
            Assert.AreNotEqual(firstResult.CachedDate, secondResult.CachedDate);
        }

    }
}
