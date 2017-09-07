using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class GenericTest
    {
        public class DataItem
        {
            public string ID { get; set; }
        }

        public interface IRepository<T>
        {
            T Get(string id);

            T Default { get; }
        }

        private class DataItemRepository : IRepository<DataItem>
        {
            public DataItem Default => new DataItem { ID = "Default" };

            public DataItem Get(string id)
            {
                return new DataItem { ID = id };
            }
        }

        [TestMethod]
        public void TestCacheKeyIsNotGeneric_Method()
        {
            IRepository<DataItem> repository = new DataItemRepository();
            var cache = new Simple.FluentDictionaryCache().WithSource(repository);

            string id = "test";
            var strat = cache.Method(c => c.Get(id));



            Assert.IsFalse(strat.Region.Contains("IRepository`1"), "The region should be based on the concrete type DataItemRepository and not the calling type in the expression IRepository<DataItem>");
        }

        [TestMethod]
        public void TestCacheKeyIsNotGeneric_Member_Property()
        {
            IRepository<DataItem> repository = new DataItemRepository();
            var cache = new Simple.FluentDictionaryCache().WithSource(repository);
            var strat = cache.Method(c => c.Default);
            Assert.IsFalse(strat.Region.Contains("IRepository`1"), "The region should be based on the concrete type DataItemRepository and not the calling type in the expression IRepository<DataItem>");
        }




    }
}
