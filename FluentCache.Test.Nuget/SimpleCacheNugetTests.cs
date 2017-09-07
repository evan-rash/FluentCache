using FluentCache.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test.Nuget
{
    [TestClass]
    public class SimleCacheTests
    {
        [TestMethod]
        public async Task Nuget_SimpleCache()
        {
            Func<ICache> factory = () => new FluentDictionaryCache();
            await CacheTester.TestCacheAsync(factory);
        }
    }
}
