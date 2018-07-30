using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentCache.Strategies;
using System.Reflection;

namespace FluentCache.Test.Implementations
{
    public class CacheTester
    {
        public class Example
        {
            public double CalculateSomeWork()
            {
                return Math.Sqrt(1234);
            }

            public double CalculateSomeWork(int power)
            {
                return Math.Pow(Math.E, power);
            }

            public async Task<double> CalculateSomeWorkAsync()
            {
                await Task.Delay(TimeSpan.FromSeconds(.125));
                return CalculateSomeWork();
            }

            public async Task<double> CalculateSomeWorkAsync(int power)
            {
                await Task.Delay(TimeSpan.FromSeconds(.125));
                return CalculateSomeWork(power);

            }

        }

        internal CacheTester()
        {
        }

        public void ThisMethod_GetValue_Default(Cache<Example> cache)
        {
            double result = cache.ThisMethod()
                                 .GetValue<double>();

            Assert.AreEqual(default(double), result, "Should return default since we didn't specify a retrieval mechanism");

        }

        public void Method_Get(Cache<Example> cache)
        {
            CachedValue<double> result = cache.Method(c => c.CalculateSomeWork())
                                              .Get();

            Assert.IsNotNull(result, "Should not return null since we specified a retrieval mechanism");
        }

        public void Method_GetValue(Cache<Example> cache)
        {
            double result = cache.Method(c => c.CalculateSomeWork())
                                 .GetValue();

            Assert.AreNotEqual(default(double), "Should not return 0 since we specified a retrieval mechanism");
        }

        public void Method_Get_InitialVersion(Cache<Example> cache)
        {
            var strat = cache.Method(c => c.CalculateSomeWork());
            strat.ClearValue();

            CachedValue<double> result = strat.Get();

            Assert.AreEqual(0L, result.Version, "Initial Get should have 0 as the version");

        }

        public void Method_Get_MultipleSameVersion(Cache<Example> cache)
        {
            CacheStrategy<double> cacheStrategy = cache.Method(c => c.CalculateSomeWork());
            for (int i = 0; i < 10; i++)
            {
                CachedValue<double> result = cacheStrategy.Get();
                Assert.AreEqual(0L, result.Version, "All subsequent calls should retrieve the existing version");
            }
        }

        public void Method_Get_Invalidation(Cache<Example> cache)
        {
            bool isInvalid = false;
            Func<CachedValue<double>, CacheValidationResult> validate = existing =>
                {
                    if (isInvalid)
                        return CacheValidationResult.Invalid;
                    else
                        return CacheValidationResult.Unknown;
                };

            CacheStrategy<double> strategy = cache.Method(c => c.CalculateSomeWork())
                                                  .Validate(validate);

            CachedValue<double> result0 = strategy.Get();


            Assert.AreEqual(0L, result0.Version);

            isInvalid = true;
            CachedValue<double> result1 = strategy.Get();
            CachedValue<double> result2 = strategy.Get();

            Assert.AreEqual(1L, result1.Version);
            Assert.AreEqual(2L, result2.Version);

        }

        public async Task Method_ExpireAfter_VersionReset(Cache<Example> cache)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan wait = TimeSpan.FromSeconds(.5);

            CacheStrategy<double> strategy = cache.Method(c => c.CalculateSomeWork())
                                                  .ExpireAfter(wait);

            strategy.ClearValue();

            CachedValue<double> result0 = strategy.Get();

            await Task.Delay(wait + wait);

            CachedValue<double> result1 = strategy.Get();

            Assert.AreEqual(0, result0.Version, "The first call should be for a new value with version 0");
            Assert.AreNotEqual(result0.CachedDate, result1.CachedDate, "The 2nd call should have expired and caused a new item to be inserted");
            Assert.AreEqual(0, result1.Version, "The 2nd call should have expired. Expired items are removed, so we don't expect the version to have incremented");
        }
        
        public async Task Method_ExpireAfter_Callback(Cache<Example> cache)
        {
            TimeSpan wait1 = TimeSpan.FromSeconds(1.25);
            TimeSpan wait2 = TimeSpan.FromSeconds(0.75);

            Func<double, TimeSpan> waitCallback = d => d < 1.0 ? wait1 : wait2;

            CacheStrategy<double> strategy = cache.Method(c => c.CalculateSomeWork())
                                                  .ExpireAfter(waitCallback);

            DateTime firstCacheDate = default(DateTime);
            strategy.Get();
            for (int i = 1; i < 10; i++)
            {
                CachedValue<double> result = strategy.Get();
                Assert.AreEqual(0L, result.Version, "These calls should be cached");

                var wait = waitCallback(result.Value);
                await Task.Delay(TimeSpan.FromSeconds(wait.TotalSeconds / 5.0));

                firstCacheDate = result.CachedDate;
            }

            await Task.Delay(wait1 + wait2);

            CachedValue<double> expiredResult = strategy.Get();

            Assert.AreNotEqual(firstCacheDate, expiredResult.CachedDate, "This call should have expired after waiting");
        }

        public async Task Method_ExpireAfter(Cache<Example> cache)
        {
            TimeSpan wait = TimeSpan.FromSeconds(1);

            CacheStrategy<double> strategy = cache.Method(c => c.CalculateSomeWork())
                                                  .ExpireAfter(wait);

            DateTime firstCacheDate = default(DateTime);
            strategy.Get();
            for (int i = 1; i < 10; i++)
            {
                CachedValue<double> result = strategy.Get();
                Assert.AreEqual(0L, result.Version, "These calls should be cached");

                await Task.Delay(TimeSpan.FromSeconds(wait.TotalSeconds / 5.0));

                firstCacheDate = result.CachedDate;
            }

            await Task.Delay(wait);

            CachedValue<double> expiredResult = strategy.Get();

            Assert.AreNotEqual(firstCacheDate, expiredResult.CachedDate, "This call should have expired after waiting for {0}", wait);
        }

        public void Method_Parameterized(Cache<Example> cache)
        {
            double result = cache.Method(c => c.CalculateSomeWork(3))
                                 .GetValue();

            Assert.AreNotEqual(0, result, "This should be a calculated value");
        }

        public async Task Method_Async_GetValue_NotDefault(Cache<Example> cache)
        {

            CacheStrategyAsync<double> strategy = cache.Method(c => c.CalculateSomeWorkAsync(3));

            double value = await strategy.GetValueAsync();

            Assert.AreNotEqual(0d, value);
        }

        public async Task Method_Async_Parameters(Cache<Example> cache)
        {
            CacheStrategyAsync<double> strategy = cache.Method(c => c.CalculateSomeWorkAsync(3));

            CachedValue<double> result1 = await strategy.GetAsync();
            CachedValue<double> result2 = await strategy.GetAsync();

            Assert.AreEqual(result1.Version, result2.Version);
        }

        

        public static async Task TestCacheAsync(Func<ICache> cacheFactory)
        {
            CacheTester tester = new CacheTester();

            var methods = tester.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var testMethod in methods)
            {
                var cache = cacheFactory().WithSource(new Example());

                var testResult = testMethod.Invoke(tester, new object[] { cache });
                if (testResult is Task)
                {
                    await (testResult as Task);
                }
            }
        }

    }


}
