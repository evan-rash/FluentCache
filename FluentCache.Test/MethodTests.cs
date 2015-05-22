using FluentCache.Strategies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class MethodTests
    {
        public const string TestConstant = "http://www.google.com";
        public string TestField = TestConstant;
        public string TestProperty { get { return TestField; } }
        public string TestMethod()
        {
            return TestConstant;
        }

        public readonly static string TestStaticField = TestConstant;
        public static string TestStaticProperty { get { return TestStaticField; } }

        private Cache<MethodTests> CreateCache()
        {
            return new FluentCache.Simple.FluentDictionaryCache().WithSource(this);
        }

        private double CalculateSomeWork()
        {
            return Math.Sqrt(1234);
        }

        private double CalculateSomeWork(int power)
        {
            return Math.Pow(Math.E, power);
        }

        private double CalculateSomeWork(int num, string text)
        {
            return Math.Pow(Math.E, num) + text.GetHashCode();
        }

        private async Task<string> DownloadTextAsync(string url)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        } 



        [TestMethod]
        public void CacheMethod_Member()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("TestProperty")
                                                    .WithRegion("MethodTests");

            CacheStrategy cacheStrategy = cache.Method(t => t.TestProperty);

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
            Assert.AreEqual(expected.Region, cacheStrategy.Region);

        }


        [TestMethod]
        public void CacheMethod_Method_IgnoreParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithRegion("MethodTests");

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(Parameter.DoNotCache<string>()));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
            Assert.AreEqual(expected.Region, cacheStrategy.Region);
        }

        [TestMethod]
        public void CacheMethod_Method_ConstantParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(TestConstant);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(TestConstant));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_FieldParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(TestField);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(t.TestField));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_PropertyParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(TestProperty);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(t.TestProperty));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_StaticFieldParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(TestStaticField);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(TestStaticField));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);

        }

        [TestMethod]
        public void CacheMethod_Method_StaticPropertyParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(TestStaticProperty);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(TestStaticProperty));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_LocalParameter()
        {
            string url = TestConstant;

            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(url);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(url));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_ClosureParameter()
        {
            const string url = TestConstant;

            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(url);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(url));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_ArgumentParameter()
        {
            DoTestCacheMethod_KeyAndRegion_ArgumentParameter(TestConstant);
        }
        private void DoTestCacheMethod_KeyAndRegion_ArgumentParameter(string url)
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("DownloadTextAsync")
                                                    .WithParameters(url);

            CacheStrategy cacheStrategy = cache.Method(t => t.DownloadTextAsync(url));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);

        }

        [TestMethod, ExpectedException(typeof(Expressions.InvalidCachingExpressionException))]
        public void CacheMethod_Method_LambdaParameter()
        {
            var cache = CreateCache();

            Func<string> testLambda = () => TestConstant;

            //We expect this to fail because the parser doesn't know how to evaluate a lamda
            cache.Method(t => t.DownloadTextAsync(testLambda()));
        }

        [TestMethod, ExpectedException(typeof(Expressions.InvalidCachingExpressionException))]
        public void CacheMethod_Method_MethodParameter()
        {
            var cache = CreateCache();

            //We expect this to fail because the parser doesn't know how to evaluate a method
            cache.Method(t => t.DownloadTextAsync(t.TestMethod()));
        }

        [TestMethod]
        public void CacheMethod_Method_GetValue()
        {
            var cache = CreateCache();

            CacheStrategy<double> strat = cache.Method(t => t.CalculateSomeWork());

            double result = strat.GetValue();

            Assert.AreNotEqual(0, result);
        }

        [TestMethod]
        public async Task CacheMethod_Method_GetValue_IgnoreParameter()
        {
            string url = "http://www.google.com";
            string url2 = "http://www.cnn.com";

            var cache = CreateCache();

            CacheStrategyAsync<string> strat1 = cache.Method(t => t.DownloadTextAsync(Parameter.DoNotCache(url)));

            CacheStrategyAsync<string> strat2 = cache.Method(t => t.DownloadTextAsync(Parameter.DoNotCache(url2)));

            Assert.AreEqual(strat1.Key, strat2.Key, "Both strategies should have the same key because the parameter should be ignored");

            string result = await strat1.GetValueAsync();

            strat1.ClearValue();

            string result2 = await strat2.GetValueAsync();

            Assert.AreNotEqual(result, result2, "These should have returned different results because the parameter values were different");
        }

        [TestMethod]
        public async Task CacheMethod_Method_GetValueAsync()
        {
            var cache = CreateCache();

            CacheStrategyAsync<string> strat = cache.Method(t => t.DownloadTextAsync(t.TestProperty));

            string result = await strat.GetValueAsync();

            Assert.AreNotEqual(null, result);
        }

        [TestMethod]
        public void CacheMethod_Method_ClearCache()
        {
            var cache = CreateCache();

            int calculationCount = 0;
            Func<double> doSomeWork = () =>
            {
                double result = calculationCount > 0 ? 0 : Math.Sqrt(100000);
                calculationCount++;
                return result;
            };

            CacheStrategy<double> cacheStrategy = cache.Method(t => t.CalculateSomeWork())
                                                        .RetrieveUsing(doSomeWork);

            Assert.AreNotEqual(default(double), cacheStrategy.GetValue());
            Assert.AreEqual(1, calculationCount, "Should have executed doSomeWork() once");

            cacheStrategy.ClearValue();

            Assert.AreEqual(0, cacheStrategy.GetValue());
            Assert.AreEqual(2, calculationCount, "Should have executed doSomeWork() again");
        }

        [TestMethod]
        public void CacheMethod_Method_SetCache()
        {
            var cache = CreateCache();

            double val = 100d;

            CacheStrategy<double> strat = cache.Method(t => t.CalculateSomeWork());

            strat.SetValue(100d);
            Assert.AreEqual(val, strat.GetValue());
        }
    }
}
