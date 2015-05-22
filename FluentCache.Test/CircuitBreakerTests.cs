using FluentCache.Strategies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    [TestClass]
    public class CircuitBreakerTests
    {
        private class SimpleCircuitBreaker : CircuitBreaker.ICircuitBreakerState
        {
            public Exception LastException { get; set; }

            public bool IsBroken { get; set; }

            public void Reset()
            {
                IsBroken = false;
            }

            public void TryBreak(Exception ex)
            {
                IsBroken = true;
                LastException = ex;
            }
        }

        private class CircuitBreakingFluentDictionaryCache : Simple.FluentDictionaryCache
        {
            public CircuitBreakingFluentDictionaryCache(CircuitBreaker.ICircuitBreakerState circuitBreaker)
            {
                CircuitBreaker = circuitBreaker;
            }

            public bool ShouldThrowException { get; set; }

            private readonly CircuitBreaker.ICircuitBreakerState CircuitBreaker;

            public override Execution.ICacheExecutionPlan<T> CreateExecutionPlan<T>(ICacheStrategy<T> cacheStrategy)
            {
                var exceptionHandler = Execution.CacheExceptionHandler.FromPredicate(ex => ShouldThrowException);
                return new Execution.CircuitBreakerCacheExecutionPlan<T>(this, exceptionHandler, cacheStrategy,  CircuitBreaker);
            }

            public override CachedValue<T> Get<T>(string key, string region)
            {
                if (ShouldThrowException)
                    throw new FluentCacheException("Test Failure");
                else
                    return base.Get<T>(key, region);
            }
        }

        [TestMethod]
        public void CircuitBreaker_ReturnsOnOpenCircuit()
        {
            var circuitBreaker = new SimpleCircuitBreaker();
            var cache = new CircuitBreakingFluentDictionaryCache(circuitBreaker)
                                .WithSource(new CacheMe());

            CacheStrategy<double> cacheStrategy = cache.Method(r => r.DoWork());


            Assert.IsFalse(circuitBreaker.IsBroken, "circuit should be open");

            CachedValue<double> result1 = cacheStrategy.Get();
            Assert.IsNotNull(result1, "because the circuit is open, we should have gotten a result");

            CachedValue<double> result2 = cacheStrategy.Get();
            Assert.IsNotNull(result2, "because the circuit is open, we should have gotten a result");
            Assert.AreEqual(result1.Version, result2.Version, "the first and 2nd results should be the same");
        }

        [TestMethod]
        public void CircuitBreaker_NullOnBrokenCircuit()
        {
            var circuitBreaker = new SimpleCircuitBreaker();
            var cache = new CircuitBreakingFluentDictionaryCache(circuitBreaker)
                                .WithSource(new CacheMe());

            CacheStrategy<double> cacheStrategy = cache.Method(r => r.DoWork());


            Assert.IsFalse(circuitBreaker.IsBroken, "circuit should be open");

            CachedValue<double> result1 = cacheStrategy.Get();
            Assert.IsNotNull(result1, "because the circuit is open, we should have gotten a result");

            circuitBreaker.TryBreak(new InvalidOperationException());
            Assert.IsTrue(circuitBreaker.IsBroken, "circuit should be closed");

            CachedValue<double> result2 = cacheStrategy.Get();
            Assert.IsNull(result2, "because the circuit is closed, we should have gotten no result");
        }

        [TestMethod]
        public void CircuitBreaker_NullOnBrokenCircuitDueToCachingFailure()
        {
            var circuitBreaker = new SimpleCircuitBreaker();
            var rawCache = new CircuitBreakingFluentDictionaryCache(circuitBreaker);
            var cache = rawCache.WithSource(new CacheMe());

            CacheStrategy<double> cacheStrategy = cache.Method(r => r.DoWork());


            Assert.IsFalse(circuitBreaker.IsBroken, "circuit should be open");

            CachedValue<double> result1 = cacheStrategy.Get();
            Assert.IsNotNull(result1, "because the circuit is open, we should have gotten a result");

            //Simulate failure
            rawCache.ShouldThrowException = true;
            
            CachedValue<double> result2 = cacheStrategy.Get();

            Assert.IsTrue(circuitBreaker.IsBroken, "circuit should now be broken");
            Assert.IsNull(result2, "because the circuit is closed, we should have gotten no result");
        }

        public class CacheMe
        {
            public double DoWork()
            {
                return Math.Exp(10);
            }
        }
    }
}
