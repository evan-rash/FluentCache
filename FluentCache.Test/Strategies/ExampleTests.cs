using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class ExampleTests
    {
        private Cache<ExampleTestsRepository> CreateCache()
        {
            return new FluentCache.Simple.FluentDictionaryCache().WithSource(new ExampleTestsRepository());
        }

        [TestMethod]
        public async Task BasicExample()
        {
            Cache<ExampleTestsRepository> cache = CreateCache();

            //Here's an example of some typical caching code
            //I want to retrieve a value from my cache, and if it's not there load it from the repository 
            var repository = new ExampleTestsRepository();
            int parameter = 5;
            string region = "FluentCacheExamples";
            string cacheKey = "Samples.DoSomeHardParameterizedWork." + parameter;

            CachedValue<double> cachedValue = cache.Get<double>(cacheKey, region);
            if (cachedValue == null)
            {
                double val = repository.DoSomeHardParameterizedWork(parameter);
                cachedValue = cache.Set<double>(cacheKey, region, val, new CacheExpiration());
            }
            double result = cachedValue.Value;

            //This code is full of boilerplate, magic strings, and is hard to read!
            //The *intent* of the code is overwhelmed by the mechanics of how to cache the value
            //I hope the method names and parameters don't change, otherwise I have to remember to update the cache key!

            //Here's the equivalent code using FluentCache
            //FluentCache automatically analyzes the expression tree an generates a unique cache key from the type, method, and any parameters
            double ezResult = cache.Method(r => r.DoSomeHardParameterizedWork(parameter))
                                   .GetValue();

            //Here's some more FluentCache examples

            //You can specify cache expiration policies
            double ttlValue = cache.Method(r => r.DoSomeHardWork())
                                   .ExpireAfter(TimeSpan.FromMinutes(5))
                                   .GetValue();

            //You can specify dynamic cache expiration policies
            double ttlValue2 = cache.Method(r => r.DoSomeHardWork())
                                    .ExpireAfter(d => d <= 2.0 ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(2))
                                    .GetValue();

            //It supports asyn/await natively
            double asyncValue = await cache.Method(r => r.DoSomeHardWorkAsync())
                                           .GetValueAsync();

            //You can specify validation strategies to customize when caches should be updated
            double onlyCachePositiveValues = cache.Method(r => r.DoSomeHardWork())
                                                  .InvalidateIf(cachedVal => cachedVal.Value <= 0d)
                                                  .GetValue();

            //You can clear existing cached values
            cache.Method(r => r.DoSomeHardParameterizedWork(parameter))
                 .ClearValue();

            /*
            Getting Started

            To get started, you need to choose a FluentCache implementation
            FluentCache supports System.Runtime.Caching.MemoryCache out of the box
            Other cache types can implement the FluentCache.ICache interface
            
            In this example, we will use the Dictionarycache, which is a simple wrapper around a ConcurrentDictionary 
            
            */
            ICache myCache = new FluentCache.Simple.FluentDictionaryCache();
            
            //Now that we have our cache, we're going to create a wrapper around our Repository
            //The wrapper will allow us to cache the results of various Repository methods
            var repo = new ExampleTestsRepository();
            Cache<ExampleTestsRepository> myRepositoryCache = myCache.WithSource(repo);

            //Now that we have a wrapper, we can create and execute a CacheStrategy
            string resource = myRepositoryCache.Method(r => r.RetrieveResource())
                                               .ExpireAfter(TimeSpan.FromMinutes(30))
                                               .GetValue();
        }
    }

    public class ExampleTestsRepository
    {
        public double DoSomeHardWork()
        {
            return Math.Exp(10);
        }

        public async Task<double> DoSomeHardWorkAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return DoSomeHardWork();
        }

        public double DoSomeHardParameterizedWork(int parameter)
        {
            return Math.Exp(parameter);
        }

        public string RetrieveResource()
        {
            return "This is just a test";
        }
    }
}
