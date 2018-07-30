using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Strategies
{
    /// <summary>
    /// A strategy to asynchronously access or modify strongly-typed data in a Cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheStrategyAsync<T> : SingleValueCacheStrategy, ICacheStrategyAsync<T>
    {
        internal CacheStrategyAsync(ICache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        internal Func<CachedValue<T>, Task<CacheValidationResult>> ValidateCallback { get; set; }
        internal Func<Task<T>> RetrieveCallback { get; set; }
        internal Func<Exception, CachedValue<T>, RetrievalErrorHandlerResult<T>> RetrieveErrorHandler { get; set; }

        /// <summary>
        /// Asynchronously gets the cached value wrapper from the cache
        /// </summary>
        /// <returns>A task containing the cached value wrapper</returns>
        public async Task<CachedValue<T>> GetAsync()
        {
            Execution.ICacheExecutionPlan<T> plan = Cache.CreateExecutionPlan<T>(this);
            return await plan.ExecuteAsync();
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns false
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategyAsync<T> InvalidateIf(Func<CachedValue<T>, bool> validate)
        {
            Func<CachedValue<T>, Task<bool>> val = existing => Task.FromResult(validate(existing));
            return InvalidateIfAsync(val);
        }

        /// <summary>
        /// Invalidates the cached value if the specified asynchronous validation delegate returns false
        /// </summary>
        /// <param name="validate">An asynchronous delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategyAsync<T> InvalidateIfAsync(Func<CachedValue<T>, Task<bool>> validate)
        {
            Func<CachedValue<T>, Task<CacheValidationResult>> val = async existing => (await validate(existing))? CacheValidationResult.Valid : CacheValidationResult.Invalid;
            return ValidateAsync(val);
        }

        /// <summary>
        /// Invalidates the cached value if the specified asynchronous validation delegate returns CacheValidationResult.Invalid
        /// </summary>
        /// <param name="validate">An asynchronous delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategyAsync<T> ValidateAsync(Func<CachedValue<T>, Task<CacheValidationResult>> validate)
        {
            this.ValidateCallback = validate;
            return this;
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns CacheValidationResult.Invalid
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategyAsync<T> Validate(Func<CachedValue<T>, CacheValidationResult> validate)
        {
            Func<CachedValue<T>, Task<CacheValidationResult>> val = existing => Task.FromResult(validate(existing));
            return ValidateAsync(val);
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">An asynchronous delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategyAsync<T> RetrieveUsingAsync(Func<Task<T>> retrieve)
        {
            this.RetrieveCallback = retrieve;
            return this;
        }

        /// <summary>
        /// Specifies an error handling strategy if retrieval fails
        /// </summary>
        /// <param name="errorHandler">An error handler that takes the exception and previous cached value (if it exists) and returns the fallback value</param>
        /// <returns>An updated cache strategy that includes the retrieval error handler strategy</returns>
        public CacheStrategyAsync<T> IfRetrievalFails(Func<Exception, CachedValue<T>, RetrievalErrorHandlerResult<T>> errorHandler)
        {
            this.RetrieveErrorHandler = errorHandler;
            return this;
        }

        /// <summary>
        /// Asynchronously gets the cached value
        /// </summary>
        /// <returns>A task containing the cached value</returns>
        public async Task<T> GetValueAsync()
        {
            CachedValue<T> cachedValue = await this.GetAsync();
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

        string ICacheStrategy<T>.Key
        {
            get { return Key; }
        }

        string ICacheStrategy<T>.Region
        {
            get { return Region; }
        }


        CacheValidationResult ICacheStrategy<T>.Validate(CachedValue<T> existingValue)
        {
            throw new NotImplementedException();
        }

        T ICacheStrategy<T>.Retrieve(CachedValue<T> existingCachedValue)
        {
            throw new NotImplementedException();
        }

        async Task<T> ICacheStrategyAsync<T>.RetrieveAsync(CachedValue<T> existingCachedValue)
        {
            if (RetrieveCallback == null)
                return default(T);

            if (RetrieveErrorHandler == null)
            {
                return await RetrieveCallback();
            }
            else
            {
                try
                {
                    return await RetrieveCallback();
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

        async Task<CacheValidationResult> ICacheStrategyAsync<T>.ValidateAsync(CachedValue<T> existingCachedValue)
        {
            if (ValidateCallback == null)
                return CacheValidationResult.Unknown;

            return await ValidateCallback(existingCachedValue);
        }
    }
}
