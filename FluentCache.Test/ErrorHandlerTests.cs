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
    public class ErrorHandlerTests
    {
        private Cache<CircuitBreakerOperations> CreateCache()
        {
            return new FluentCache.Simple.FluentDictionaryCache().WithSource(new CircuitBreakerOperations());
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ErrorHandling_NoErrorHandling()
        {
            CacheStrategy<int> cacheStrategy = CreateCache().Method(c => c.ThrowsExceptionImmediately())
                                                   .IfRetrievalFailsUsePreviousValue();

            cacheStrategy.GetValue();
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public async Task ErrorHandling_Async_NoErrorHandling()
        {
            CacheStrategyAsync<int> cacheStrategy = CreateCache().Method(c => c.ThrowsExceptionImmediatelyAsync())
                                                   .IfRetrievalFailsUsePreviousValue();

            await cacheStrategy.GetValueAsync();
        }

        [TestMethod]
        public void ErrorHandling_UsePreviousValue()
        {
            CacheStrategy<int> cacheStrategy = CreateCache().Method(c => c.RandomValueThatThrowsExceptionOnSecondTry())
                                                            .IfRetrievalFailsUsePreviousValue();

            int previousValue = cacheStrategy.GetValue();

            int newValue = cacheStrategy.GetValue();

            Assert.AreEqual(previousValue, newValue, "The retrieval error handler should have kicked in");
        }

        [TestMethod]
        public async Task ErrorHandling_Async_UsePreviousValue()
        {
            bool isInvalid = false;
            Func<CachedValue<double>, CacheValidationResult> validate = val => isInvalid ? CacheValidationResult.Invalid : CacheValidationResult.Unknown;

            CacheStrategyAsync<int> cacheStrategy = CreateCache().Method(c => c.RandomValueThatThrowsExceptionOnSecondTryAsync())
                                                                 .IfRetrievalFailsUsePreviousValue();

            int previousValue = await cacheStrategy.GetValueAsync();

            isInvalid = true;   //force a second retrieval by invalidating the previous value

            int newValue = await cacheStrategy.GetValueAsync();

            Assert.AreEqual(previousValue, newValue, "The retrieval error handler should have kicked in");
        }

        [TestMethod]
        public void ErrorHandling_UsePreviousValueOrDefault()
        {
            int defaultValue = -1111;

            CacheStrategy<int> cacheStrategy = CreateCache().Method(c => c.ThrowsExceptionImmediately())
                                                            .IfRetrievalFailsUsePreviousValueOrDefault(defaultValue);

            int value = cacheStrategy.GetValue();

            Assert.AreEqual(defaultValue, value, "The retrieval error handler should have kicked in and returned the default value");

        }

        [TestMethod]
        public async Task ErrorHandling_Async_UsePreviousValueOrDefault()
        {
            int defaultValue = -1111;

            CacheStrategyAsync<int> cacheStrategy = CreateCache().Method(c => c.ThrowsExceptionImmediatelyAsync())
                                                                 .IfRetrievalFailsUsePreviousValueOrDefault(defaultValue);

            int value = await cacheStrategy.GetValueAsync();

            Assert.AreEqual(defaultValue, value, "The retrieval error handler should have kicked in and returned the default value");
        }
    }


    public class CircuitBreakerOperations
    {
        public CircuitBreakerOperations()
        {
            _Random = new Random(0);
        }

        private int _OperationCount = 0;
        private Random _Random;

        public int RandomValueThatThrowsExceptionOnSecondTry()
        {
            if (_OperationCount > 0)
                throw new InvalidOperationException("OperationCount > 0");

            _OperationCount++;

            return _Random.Next();
        }

        public int ThrowsExceptionImmediately()
        {
            throw new InvalidOperationException("Not Implemented");
        }

        public async Task<int> RandomValueThatThrowsExceptionOnSecondTryAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(.125));

            if (_OperationCount > 0)
                throw new InvalidOperationException("OperationCount > 0");

            _OperationCount++;

            return _Random.Next();
        }

        public async Task<int> ThrowsExceptionImmediatelyAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(.125));

            throw new InvalidOperationException("Not Implemented");
        }
    }
}
