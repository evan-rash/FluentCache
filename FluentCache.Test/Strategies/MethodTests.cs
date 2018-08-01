using FluentCache.Expressions;
using FluentCache.Strategies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    public class MethodTestsArguments
    {
        public const string Constant = "boo!";
        public string Field = Constant;

        public string Property { get { return Field; } }
        public string Method()
        {
            return Constant;
        }

        public readonly static string StaticField = Constant;

        public static string StaticProperty { get { return StaticField; } }
    }

    public class NestedProperty
    {
        public string Property1 => "Hello";
        public string Property2 => "World";
    }

    public class MethodTestsRepository
    {
        public MethodTestsRepository()
        {
            Field = Method();
        }

        public double Field;
        public double Property => Method();

        public NestedProperty NestedProperty => new NestedProperty();

        public double Method()
        {
            return Math.Sqrt(1234);
        }

        public double Method1Parameter(int power)
        {
            return Math.Pow(Math.E, power);
        }

        public double MethodTwoParameters(int num, string text)
        {
            return Math.Pow(Math.E, num) + text.GetHashCode();
        }

        public async Task<string> MethodAsync(string text)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.25));
            return text;
        }

        public int MethodEnumerableParameter<T>(IEnumerable<T> item)
        {
            return item == null ? 0 : item.Count();
        }
    }

    [TestClass]
    public class MethodTests
    {

        private Cache<MethodTestsRepository> CreateCache()
        {
            return new FluentCache.Simple.FluentDictionaryCache().WithSource(new MethodTestsRepository());
        }

        
        [TestMethod]
        public void CacheMethod_Member_Property()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("Property")
                                                    .WithRegion(nameof(MethodTestsRepository));

            CacheStrategy cacheStrategy = cache.Method(r => r.Property);

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
            Assert.AreEqual(expected.Region, cacheStrategy.Region);

        }

        [TestMethod, ExpectedException(typeof(InvalidCachingExpressionException))]
        public void CacheMethod_Member_NestedProperty()
        {
            var cache = CreateCache();

            //we expect this to fail because caching of nested properties is not supported
            CacheStrategy cacheStrategy = cache.Method(r => r.NestedProperty.Property1);

        }

        [TestMethod]
        public void CacheMethod_Member_Field()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("Field")
                                                    .WithRegion(nameof(MethodTestsRepository));

            CacheStrategy cacheStrategy = cache.Method(r => r.Field);

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
            Assert.AreEqual(expected.Region, cacheStrategy.Region);

        }


        [TestMethod]
        public void CacheMethod_Method_IgnoreParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithRegion(nameof(MethodTestsRepository));

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(Parameter.DoNotCache<string>()));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
            Assert.AreEqual(expected.Region, cacheStrategy.Region);
        }

        [TestMethod]
        public void CacheMethod_Method_ConstantParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(MethodTestsArguments.Constant);

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(MethodTestsArguments.Constant));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_FieldParameter()
        {
            var cache = CreateCache();

            var args = new MethodTestsArguments();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(args.Field);

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(args.Field));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_PropertyParameter()
        {
            var cache = CreateCache();

            var args = new MethodTestsArguments();
            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(args.Property);

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(args.Property));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_StaticFieldParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(MethodTestsArguments.StaticField);

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(MethodTestsArguments.StaticField));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);

        }

        [TestMethod]
        public void CacheMethod_Method_StaticPropertyParameter()
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(MethodTestsArguments.StaticProperty);

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(MethodTestsArguments.StaticProperty));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_LocalParameter()
        {
            string localParameter = MethodTestsArguments.Constant;

            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(localParameter);

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(localParameter));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_ClosureParameter()
        {
            const string localParameter = MethodTestsArguments.Constant;

            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(localParameter);

            CacheStrategy cacheStrategy = cache.Method(t => t.MethodAsync(localParameter));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_LambdaParameter()
        {
            var cache = CreateCache();

            Func<string> testLambda = () => MethodTestsArguments.Constant;


            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                   .WithParameters(testLambda());

            CacheStrategy cacheStrategy = cache.Method(t => t.MethodAsync(testLambda()));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_MethodParameter()
        {
            var args = new MethodTestsArguments();
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                   .WithParameters(args.Method());

            CacheStrategy cacheStrategy = cache.Method(t => t.MethodAsync(args.Method()));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);
        }

        [TestMethod]
        public void CacheMethod_Method_ArgumentParameter()
        {
            DoTestCacheMethod_KeyAndRegion_ArgumentParameter(MethodTestsArguments.Constant);
        }
        private void DoTestCacheMethod_KeyAndRegion_ArgumentParameter(string argument)
        {
            var cache = CreateCache();

            CacheStrategyIncomplete expected = cache.WithKey("MethodAsync")
                                                    .WithParameters(argument);

            CacheStrategy cacheStrategy = cache.Method(r => r.MethodAsync(argument));

            Assert.AreEqual(expected.Key, cacheStrategy.Key);

        }

        [TestMethod]
        public void CacheMethod_Method_EnumerableParameter()
        {
            var cache = CreateCache();

            var arg1 = new List<string> { "a", "b", "c" };
            var arg2 = new List<string> { "d", "e", "f", "g" };
            var arg3 = new string[] { "a", "b", "c" };


            var strat1 = cache.Method(r => r.MethodEnumerableParameter(arg1));
            var strat2 = cache.Method(r => r.MethodEnumerableParameter(arg2));
            var strat3 = cache.Method(r => r.MethodEnumerableParameter(arg3));


            Assert.IsTrue(strat1.Key.Contains("[a,b,c]"), "IEnumerable<string> parameter should be rendered as a list");
            Assert.IsTrue(strat2.Key.Contains("[d,e,f,g]"), "IEnumerable<string> parameter should be rendered as a list");
            Assert.AreEqual(strat1.Key, strat3.Key, "Both arg1 and arg3 are IEnumerable<string> and should have the same cache key");

            var cache1 = strat1.Get();
            var cache2 = strat2.Get();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(.5));
            var cache3 = strat3.Get();

            Assert.AreEqual(arg1.Count, cache1.Value);
            Assert.AreEqual(arg2.Count, cache2.Value);
            Assert.AreEqual(cache1.CachedDate, cache3.CachedDate);
        }

        [TestMethod]
        public void CacheMethod_Method_GetValue()
        {
            var cache = CreateCache();

            CacheStrategy<double> strat = cache.Method(r => r.Method());

            double result = strat.GetValue();

            Assert.AreNotEqual(0, result);
        }

        [TestMethod]
        public async Task CacheMethod_Method_GetValue_IgnoreParameter()
        {
            string arg1 = "hello";
            string arg2 = "world";

            var cache = CreateCache();

            CacheStrategyAsync<string> strat1 = cache.Method(t => t.MethodAsync(Parameter.DoNotCache(arg1)));

            CacheStrategyAsync<string> strat2 = cache.Method(t => t.MethodAsync(Parameter.DoNotCache(arg2)));

            Assert.AreEqual(strat1.Key, strat2.Key, "Both strategies should have the same key because the parameter should be ignored");

            string result = await strat1.GetValueAsync();

            strat1.ClearValue();

            string result2 = await strat2.GetValueAsync();

            Assert.AreNotEqual(result, result2, "These should have returned different results because the parameter values were different");
        }

        [TestMethod]
        public async Task CacheMethod_Method_GetValueAsync_PropertyArgument()
        {
            var cache = CreateCache();
            var args = new MethodTestsArguments();

            CacheStrategyAsync<string> strat = cache.Method(t => t.MethodAsync(args.Property));

            string result = await strat.GetValueAsync();

            Assert.AreNotEqual(null, result);
        }

        [TestMethod]
        public async Task CacheMethod_Method_GetValueAsync_LocalArgument()
        {
            var cache = CreateCache();
            var arg = new MethodTestsArguments().Property;

            CacheStrategyAsync<string> strat = cache.Method(t => t.MethodAsync(arg));

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

            CacheStrategy<double> cacheStrategy = cache.Method(t => t.Method())
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

            CacheStrategy<double> strat = cache.Method(t => t.Method());

            strat.SetValue(100d);
            Assert.AreEqual(val, strat.GetValue());
        }

        [TestMethod]
        public void CacheMethod_Method_Validate()
        {
            var cache = CreateCache();

            var strat1 = cache.Method(t => t.Method())
                              .Validate(cv => CacheValidationResult.Valid);

            var val1 = strat1.Get();
            var val2 = strat1.Get();
            Assert.AreEqual(0, val1.Version);
            Assert.AreEqual(0, val2.Version);
            strat1.ClearValue();

            var strat2 = cache.Method(t => t.Method())
                              .Validate(cv => CacheValidationResult.Invalid);

            var val3 = strat2.Get();
            var val4 = strat2.Get();
            Assert.AreEqual(0, val3.Version);
            Assert.AreEqual(1, val4.Version);
        }
    }
}
