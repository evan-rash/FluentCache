
using FluentCache.RuntimeCaching;
using FluentCache.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test.Implementations
{
    [TestClass]
    public class RuntimeCacheTests
    {
        [TestMethod]
        public async Task Implementation_RuntimeMemoryCache()
        {
            await CacheTester.TestCacheAsync(CreateCache);
        }

        private ICache CreateCache()
        {
            var memoryCache = System.Runtime.Caching.MemoryCache.Default;
            var cache = new FluentMemoryCache(memoryCache);
            return cache;
        }
    }
}
