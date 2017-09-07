using FluentCache.Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test.Nuget
{
    [TestClass]
    public class MemoryCacheNugetTests
    {
        [TestMethod]
        public async Task Nuget_MemoryCache()
        {
            await CacheTester.TestCacheAsync(CacheFactory);
        }

        private ICache CacheFactory()
        {
            return new FluentMemoryCache();
        }
    }
}
