using FluentCache.Microsoft.Extensions.Caching.Memory;
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
    public class MemoryCacheTests
    {
        [TestMethod]
        public async Task Implementation_MemoryCache()
        {
            await CacheTester.TestCacheAsync(CacheFactory);
        }

        private ICache CacheFactory()
        {
            return new FluentMemoryCache();
        }
    }
}
