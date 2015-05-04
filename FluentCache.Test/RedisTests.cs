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

        private ICache<CacheMe> CreateCache()
        {
            return new Redis.FluentRedisCache(Redis).WithSource(new CacheMe());
        }

        [TestMethod]
        public void Redis_SimpleCache()
        {
            var cache = CreateCache();
            var cacheStrategy = cache.Method(c => c.DoSomeWork());

            ICachedValueWithVersion<double> firstResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, firstResult.Version);

            ICachedValueWithVersion<double> secondResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, secondResult.Version);
        }

        [TestMethod]
        public void Redis_SimpleCache_Parameter()
        {
            var cache = CreateCache();

            var cacheStrategy1 = cache.Method(c => c.DoSomeWorkParameterized(1));
            ICachedValueWithVersion<double> firstResult = cacheStrategy1.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, firstResult.Version);

            var cacheStrategy2 = cache.Method(c => c.DoSomeWorkParameterized(2));
            ICachedValueWithVersion<double> secondResult = cacheStrategy2.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, secondResult.Version);

            Assert.AreNotEqual(firstResult.Value, secondResult.Value);
        }

        [TestMethod]
        public async Task Redis_ExpireAfter()
        {
            var cache = CreateCache();
            var cacheStrategy = cache.Method(c => c.DoSomeWork())
                                     .ExpireAfter(TimeSpan.FromSeconds(3));

            var start = DateTime.UtcNow;

            ICachedValueWithVersion<double> firstResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, firstResult.Version);

            await Task.Delay(TimeSpan.FromSeconds(2));

            //we should still get the same cached version of data
            ICachedValueWithVersion<double> secondResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, secondResult.Version);

            await Task.Delay(TimeSpan.FromSeconds(4));

            //we should have a new version of data because the sliding expiration has expired
            ICachedValueWithVersion<double> thirdResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;

            Assert.AreEqual(2, thirdResult.Version);
        }

        [TestMethod]
        public async Task Redis_ExpireOn()
        {
            var cache = CreateCache();

            var start = DateTime.UtcNow;
            var cacheStrategy = cache.Method(c => c.DoSomeWork())
                                     .ExpireOn(start.AddSeconds(3));

            ICachedValueWithVersion<double> firstResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, firstResult.Version);

            await Task.Delay(TimeSpan.FromSeconds(2));
            ICachedValueWithVersion<double> secondResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, secondResult.Version);

            await Task.Delay(TimeSpan.FromSeconds(3));
            ICachedValueWithVersion<double> thirdResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;

            Assert.AreNotEqual(firstResult.Version, thirdResult.Version);
        }

        [TestMethod]
        public void Redis_ClearCache()
        {
            var cache = CreateCache();
            var cacheStrategy = cache.Method(c => c.DoSomeWork());

            ICachedValueWithVersion<double> firstResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(1, firstResult.Version);

            cacheStrategy.ClearValue();

            ICachedValueWithVersion<double> secondResult = cacheStrategy.Get() as ICachedValueWithVersion<double>;
            Assert.AreEqual(2, secondResult.Version);
        }

    }
}
