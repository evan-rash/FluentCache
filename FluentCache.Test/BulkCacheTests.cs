using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class BulkCacheTests
    {
        private Cache<BulkCacheOperations> CreateCache()
        {
            return new Simple.FluentDictionaryCache().WithSource(new BulkCacheOperations());
        }

        [TestMethod]
        public async Task Bulk_CacheOperation()
        {
            var cache = CreateCache();

            var nums = new List<int> { 1 ,2, 3 };

            cache.Method(t => t.GetExponents(nums))
                 .GetAll();

            var nums2 = new List<int> { 2, 3 };

            await Task.Delay(TimeSpan.FromSeconds(.25));

            DateTime now = DateTime.UtcNow;


            IList<CachedValue<double>> results = cache.Method(t => t.GetExponents(nums2))
                                                       .GetAll();

            foreach (CachedValue<double> cachedItem in results)
            {
                Assert.IsTrue(cachedItem.CachedDate < now, "All items should have been previously cached");
            }
        }

        [TestMethod]
        public async Task Bulk_CacheOperationAsync()
        {
            var cache = CreateCache();

            var nums = new List<int> { 1, 2, 3 };

            await cache.Method(t => t.GetExponentsAsync(nums))
                       .GetAllAsync();

            var nums2 = new List<int> { 2, 3 };

            await Task.Delay(TimeSpan.FromSeconds(.25));

            DateTime now = DateTime.UtcNow;


            IList<CachedValue<double>> results = await cache.Method(t => t.GetExponentsAsync(nums2))
                                                             .GetAllAsync();

            foreach (CachedValue<double> cachedItem in results)
            {
                Assert.IsTrue(cachedItem.CachedDate < now, "All items should have been previously cached");
            }
        }

        [TestMethod, ExpectedException(typeof(Expressions.InvalidCachingExpressionException))]
        public void Bulk_FirstParameterMustBeKeyCollection()
        {
            var cache = CreateCache();

            cache.Method(t => t.GetExponentsBadArgument("evil"))
                 .GetAll();
        }

        [TestMethod]
        public async Task Bulk_CacheOperation_MultipleParameters()
        {
            var cache = CreateCache();

            var nums = new List<int> { 1, 2, 3 };

            var firstStrat = cache.Method(t => t.GetExponentsWithParameter(nums, 10));
            firstStrat.GetAll();

            await Task.Delay(TimeSpan.FromSeconds(.5));
            DateTime notCachedBefore = DateTime.UtcNow;
            await Task.Delay(TimeSpan.FromSeconds(.5));

            var secondStrat = cache.Method(t => t.GetExponentsWithParameter(nums, 11));
            IList<CachedValue<double>> results = secondStrat.GetAll();

            foreach (CachedValue<double> cachedItem in results)
            {
                Assert.IsTrue(cachedItem.CachedDate > notCachedBefore, "No items should have been cached. Item with value {0} was cached on {1} and should not have been cached before {2}", cachedItem.Value, cachedItem.CachedDate, notCachedBefore);
            }
        }

        [TestMethod]
        public async Task Bulk_CacheOperationAsync_MultipleParameters()
        {
            var cache = CreateCache();

            var nums = new List<int> { 1, 2, 3 };

            cache.Method(t => t.GetExponentsWithParameter(nums, 10))
                 .GetAll();

            await Task.Delay(TimeSpan.FromSeconds(.5));
            DateTime notCachedBefore = DateTime.UtcNow;
            await Task.Delay(TimeSpan.FromSeconds(.5));

            IList<CachedValue<double>> results = cache.Method(t => t.GetExponentsWithParameter(nums, 11))
                                                       .GetAll();

            foreach (CachedValue<double> cachedItem in results)
            {
                Assert.IsTrue(cachedItem.CachedDate > notCachedBefore, "No items should have been cached. Item with value {0} was cached on {1} and should not have been cached before {2}", cachedItem.Value, cachedItem.CachedDate, notCachedBefore);
            }
        }

        [TestMethod]
        public async Task Bulk_CacheOperation_Clear()
        {
            var cache = CreateCache();

            var nums = new List<int> { 1, 2, 3 };

            var strat = cache.Method(t => t.GetExponentsWithParameter(nums, 10));

            strat.GetAll();

            await Task.Delay(TimeSpan.FromSeconds(.5));
            DateTime notCachedBefore = DateTime.UtcNow;
            await Task.Delay(TimeSpan.FromSeconds(.5));

            strat.ClearValues();

            IList<CachedValue<double>> results = strat.GetAll();

            foreach (CachedValue<double> cachedItem in results)
                Assert.IsTrue(cachedItem.CachedDate > notCachedBefore, "No items should have been cached. Item with value {0} was cached on {1} and should not have been cached before {2}", cachedItem.Value, cachedItem.CachedDate, notCachedBefore);
        }


    }

    public class BulkCacheOperations
    {
        public ICollection<KeyValuePair<int, double>> GetExponentsBadArgument(string input)
        {
            throw new NotImplementedException();
        }

        public ICollection<KeyValuePair<int, double>> GetExponentsWithParameter(ICollection<int> indexes, double baseValue)
        {
            return indexes.Select(indx => new { Index = indx, Value = Math.Pow(baseValue, indx) })
                          .ToDictionary(x => x.Index, x => x.Value);
        }

        public async Task<ICollection<KeyValuePair<int, double>>> GetExponentsWithParameterAsync(ICollection<int> indexes, double baseValue)
        {
            await Task.Delay(TimeSpan.FromSeconds(.125));
            return GetExponentsWithParameter(indexes, baseValue);
        }

        public ICollection<KeyValuePair<int, double>> GetExponents(ICollection<int> indexes)
        {
            return indexes.Select(indx => new { Index = indx, Value = Math.Exp(indx) })
                          .ToDictionary(x => x.Index, x => x.Value);
                          
        }

        public async Task<ICollection<KeyValuePair<int, double>>> GetExponentsAsync(ICollection<int> indexes)
        {
            await Task.Delay(TimeSpan.FromSeconds(.75));

            return GetExponents(indexes);
        }
    }
}
