
using FluentCache.Microsoft.Extensions.Caching.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test.Implementations
{
    [TestClass]
    public class RedisCacheTests
    {
        [TestMethod]
        public async Task Implementation_RedisCache()
        {
            await CacheTester.TestCacheAsync(CacheFactory);
        }

        private ICache CacheFactory()
        {
            var keys = JToken.Parse(System.IO.File.ReadAllText(@"c:\code\keys\fluentcachetest.json"));
            string instance = keys.Value<string>("instance");
            string configuration = keys.Value<string>("configuration");

            return new FluentRedisCache(instance, configuration);
        }
    }
}
