using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Strategies
{
    /// <summary>
    /// A strategy to access or modify strongly-typed data in a Cache
    /// </summary>
    /// <typeparam name="T">The type of the data in the Cache</typeparam>
    public class CacheStrategy<T> : SingleValueCacheStrategy, ICacheStrategy<T>
    {
        internal CacheStrategy(ICache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        internal Func<CachedValue<T>, CacheValidationResult> ValidateCallback { get; set; }
        internal Func<T> RetrieveCallback { get; set; }
        internal Func<Exception, CachedValue<T>, RetrievalErrorHandlerResult<T>> RetrieveErrorHandler { get; set; }

        /// <summary>
        /// Gets the cached value wrapper from the cache
        /// </summary>
        /// <returns>The cached value wrapper</returns>
        public CachedValue<T> Get()
        {
            Execution.ICacheExecutionPlan<T> plan = Cache.CreateExecutionPlan(this);
            return plan.Execute();
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns false
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategy<T> InvalidateIf(Func<CachedValue<T>, bool> validate)
        {
            Func<CachedValue<T>, CacheValidationResult> val = existing => validate(existing) ? CacheValidationResult.Valid : CacheValidationResult.Invalid;
            return Validate(val);
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns CacheValidationResult.Invalid
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>

        public CacheStrategy<T> Validate(Func<CachedValue<T>, CacheValidationResult> validate)
        {
            this.ValidateCallback = validate;
            return this;
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategy<T> RetrieveUsing(Func<T> retrieve)
        {
            this.RetrieveCallback = retrieve;
            return this;
        }

        /// <summary>
        /// Specifies an error handling strategy if retrieval fails
        /// </summary>
        /// <param name="errorHandler">An error handler that takes the exception and previous cached value (if it exists) and returns the fallback value</param>
        /// <returns>An updated cache strategy that includes the retrieval error handler strategy</returns>
        public CacheStrategy<T> IfRetrievalFails(Func<Exception, CachedValue<T>, RetrievalErrorHandlerResult<T>> errorHandler)
        {
            this.RetrieveErrorHandler = errorHandler;
            return this;
        }

        /// <summary>
        /// Gets the cached value
        /// </summary>
        /// <returns>The cached value</returns>
        public T GetValue()
        {
            CachedValue<T> cachedValue = this.Get();
            return cachedValue == null ? default(T) : cachedValue.Value;
        }

        /// <summary>
        /// Sets the cached value
        /// </summary>
        /// <param name="value">The value to be cached</param>
        public void SetValue(T value)
        {
            Cache.Set(Key, Region, value, ResolveExpiration(value));
        }

        /// <summary>
        /// Resolves the Expiration that this caching policy will use when caching items
        /// </summary>
        public CacheExpiration ResolveExpiration(T value)
        {
            return base.ResolveExpiration(value);

        }

        CacheValidationResult ICacheStrategy<T>.Validate(CachedValue<T> existingValue)
        {
            if (ValidateCallback == null)
                return CacheValidationResult.Unknown;

            return ValidateCallback(existingValue);
        }

        T ICacheStrategy<T>.Retrieve(CachedValue<T> existingCachedValue)
        {
            if (RetrieveCallback == null)
                return default(T);

            if (RetrieveErrorHandler == null)
            {
                return RetrieveCallback();
            }
            else
            {
                try
                {
                    return RetrieveCallback();
                }
                catch (Exception x)
                {
                    RetrievalErrorHandlerResult<T> result = RetrieveErrorHandler(x, existingCachedValue);
                    if (result.IsErrorHandled)
                        return result.FallbackResult;
                    else
                        throw;
                }
            }
        }
    }






}
