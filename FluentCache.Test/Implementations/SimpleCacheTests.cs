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
    public class SimleCacheTests
    {
        [TestMethod]
        public async Task Implementation_SimpleCache()
        {
            Func<ICache> factory = () => new FluentDictionaryCache();
            await CacheTester.TestCacheAsync(factory);
        }
    }
}
