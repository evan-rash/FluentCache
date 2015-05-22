using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Execution
{
    /// <summary>
    /// Defines an execution plan for getting a value from a cache, validating, and retrieving a new value
    /// </summary>
    /// <typeparam name="T">The type of value that is cached</typeparam>
    public class CircuitBreakerCacheExecutionPlan<T> : CacheExecutionPlan<T>
    {
        /// <summary>
        /// Constructs a new instance of the execution plan
        /// </summary>
        public CircuitBreakerCacheExecutionPlan(Cache cache, ICacheExceptionHandler exceptionHandler, ICacheStrategy<T> cacheStrategy, CircuitBreaker.ICircuitBreakerState circuitBreaker)
            : base(cache, exceptionHandler, cacheStrategy)
        {
            _circuitBreaker = circuitBreaker;
        }

        /// <summary>
        /// Constructs a new instance of the execution plan using the default circuit breaker
        /// </summary>
        public CircuitBreakerCacheExecutionPlan(Cache cache, ICacheExceptionHandler exceptionHandler, ICacheStrategy<T> cacheStrategy)
            : this(cache, exceptionHandler, cacheStrategy, GetDefaultCircuitBreaker())
        {
        }

        private readonly CircuitBreaker.ICircuitBreakerState _circuitBreaker;

        private static CircuitBreaker.ICircuitBreakerState GetDefaultCircuitBreaker()
        {
            return new CircuitBreaker.CircuitBreakerState(exceptionsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Executes the plan
        /// </summary>
        public override CachedValue<T> Execute()
        {
            //If the circuit is broken then we won't even try to retrieve the value from the cache
            if (_circuitBreaker.IsBroken)
                return null;

            CachedValue<T> cachedValue = null;
            try
            {
                //Get the value from the cache
                cachedValue = Cache.Get<T>(Key, Region);

                //validate the value
                CacheValidationResult validationResult = ValidateCachedValue(cachedValue);

                //if the value is invalid or non-existing then retrieve the value from the cache
                if (validationResult != CacheValidationResult.Valid && (validationResult == CacheValidationResult.Invalid || cachedValue == null))
                    cachedValue = RetrieveCachedValue(previousCachedValue: cachedValue);

                //We successfully retrieved the value from the cache, so reset the circuit
                _circuitBreaker.Reset();

                return cachedValue;
            }
            catch (FluentCache.FluentCacheException cacheException)
            {
                //attempt to handle the caching failure
                if (ExceptionHandler.TryHandleCachingFailure(cacheException))
                {
                    //try to break the circuit
                    _circuitBreaker.TryBreak(cacheException);
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Asynchronously executes the plan
        /// </summary>
        public override async Task<CachedValue<T>> ExecuteAsync()
        {
            //If the circuit is broken then we won't even try to retrieve the value from the cache
            if (_circuitBreaker.IsBroken)
                return null;

            CachedValue<T> cachedValue = null;
            try
            {
                //Get the value from the cache
                cachedValue = Cache.Get<T>(Key, Region);

                //validate the value
                CacheValidationResult validationResult = await ValidateCachedValueAsync(cachedValue);

                //if the value is invalid or non-existing then retrieve the value from the cache
                if (validationResult != CacheValidationResult.Valid && (validationResult == CacheValidationResult.Invalid || cachedValue == null))
                    cachedValue = await RetrieveCachedValueAsync(previousCachedValue: cachedValue);

                //We successfully retrieved the value from the cache, so reset the circuit
                _circuitBreaker.Reset();

                return cachedValue;
            }
            catch (FluentCache.FluentCacheException cacheException)
            {
                //attempt to handle the caching failure
                if (ExceptionHandler.TryHandleCachingFailure(cacheException))
                {
                    //try to break the circuit
                    _circuitBreaker.TryBreak(cacheException);
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        
    }

}
