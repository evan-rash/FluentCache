using FluentCache.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class ParameterCacheKeyProviderTests
    {
        [TestMethod]
        public void TestUsingNonDefaultParameterCacheKey()
        {
            var param = new ParameterWithNonDefaultCacheKey
            {
                PropertyForCacheKey = "forcachekey",
                PropertyForToString = "fortostring"
            };

            var cache = new SimpleCache();
            var cacheWithCustomParameterCacheKey = new SimpleCache(new TestParameterCacheKeyProvider());

            var cachePolicy  = cache.ThisMethod().WithParameters(param);
            var cachePolicy2 = cacheWithCustomParameterCacheKey.ThisMethod().WithParameters(param);

            Assert.AreNotEqual(cachePolicy.Key, cachePolicy2.Key);
        }
    }

    public class TestParameterCacheKeyProvider : DefaultParameterCacheKeyProvider
    {
        public override string GenerateParameterCacheKey(object parameter)
        {
            if (parameter != null && parameter is ParameterWithNonDefaultCacheKey)
            {
                return ((ParameterWithNonDefaultCacheKey)parameter).PropertyForCacheKey;
            }
            else
            {
                return base.GenerateParameterCacheKey(parameter);
            }
        }
    }

    public class ParameterWithNonDefaultCacheKey
    {
        public string PropertyForToString { get; set; }
        public string PropertyForCacheKey { get; set; }

        public override string ToString()
        {
            return PropertyForToString;
        }
    }
}
