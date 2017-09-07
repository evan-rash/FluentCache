
using FluentCache.RuntimeCaching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test.Nuget
{
    [TestClass]
    public class RuntimeCacheNugetTests
    {
        [TestMethod]
        public async Task Nuget_RuntimeMemoryCache()
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
