using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Defines an execution plan for getting a value from a cache, validating, and retrieving a new value
    /// </summary>
    /// <typeparam name="T">The type of value that is cached</typeparam>
    public class CacheExecutionPlan<T>
    {
        /// <summary>
        /// Constructs a new instance of the execution plan
        /// </summary>
        public CacheExecutionPlan(Cache cache, ICacheStrategy<T> cacheStrategy)
        {
            _cache = cache;
            _cacheStrategy = cacheStrategy;
            _cacheStrategyAsync = cacheStrategy as ICacheStrategyAsync<T>;
        }

        private readonly Cache _cache;
        private readonly ICacheStrategy<T> _cacheStrategy;
        private readonly ICacheStrategyAsync<T> _cacheStrategyAsync;

        /// <summary>
        /// Gets the Key that will be used in combination with the Region to retrieve the value from the cache
        /// </summary>
        public string Key { get { return _cacheStrategy.Key; } }

        /// <summary>
        /// Gets the Region that will be used in combination with the Key to retrieve the value from the cache
        /// </summary>
        public string Region { get { return _cacheStrategy.Region; } }
        
        /// <summary>
        /// Gets the cache that will be used to retrieve the value
        /// </summary>
        public Cache Cache { get { return _cache; } }
        
        /// <summary>
        /// Gets the expiration policy
        /// </summary>
        public CacheExpiration Expiration { get { return _cacheStrategy.Expiration; } }

        /// <summary>
        /// Attempts to handle a failure that occured during caching. Return true to indicate that the error was handled and false to indicate the error is unhandled and needs to be propogated further
        /// </summary>
        /// <param name="cacheException">The exception that occurred during caching</param>
        /// <returns>true to indicate the error was handled, false to indicate it is unhandled</returns>
        protected virtual bool TryHandleCachingFailure(FluentCacheException cacheException)
        {
            return false;
        }
        
        /// <summary>
        /// Retrieves the value if it is not in the cache
        /// </summary>
        protected virtual CachedValue<T> RetrieveCachedValue(CachedValue<T> previousCachedValue)
        {
            T value = _cacheStrategy.Retrieve(previousCachedValue);
            return Cache.Set(Key, Region, value, Expiration);
        }

        /// <summary>
        /// Asynchronously retrieves the value if it is not in the cache
        /// </summary>
        protected virtual async Task<CachedValue<T>> RetrieveCachedValueAsync(CachedValue<T> previousCachedValue)
        {
            if (_cacheStrategyAsync == null)
                throw new InvalidOperationException("Specified cache strategy must be an instance of ICacheStrategyAsync<T>");

            T value = await _cacheStrategyAsync.RetrieveAsync(previousCachedValue);
            return Cache.Set(Key, Region, value, Expiration);
        }
        
        /// <summary>
        /// Validates the cached value
        /// </summary>
        protected virtual CacheValidationResult ValidateCachedValue(CachedValue<T> existingCachedValue)
        {
            if (existingCachedValue == null)
                return CacheValidationResult.Unknown;

            CacheValidationResult result = _cacheStrategy.Validate(existingCachedValue);
            if (result == CacheValidationResult.Valid)
                Cache.MarkAsValidated(Key, Region);

            return result;
        }
        
        /// <summary>
        /// Asynchronously validates the cached value
        /// </summary>
        protected virtual async Task<CacheValidationResult> ValidateCachedValueAsync(CachedValue<T> existingCachedValue)
        {
            if (_cacheStrategyAsync == null)
                throw new InvalidOperationException("Specified cache strategy must be an instance of ICacheStrategyAsync<T>");

            if (existingCachedValue == null)
                return CacheValidationResult.Unknown;

            CacheValidationResult result = await _cacheStrategyAsync.ValidateAsync(existingCachedValue);
            if (result == CacheValidationResult.Valid)
                Cache.MarkAsValidated(Key, Region);

            return result;
        }

        /// <summary>
        /// Executes the plan
        /// </summary>
        public CachedValue<T> Execute()
        {
            CachedValue<T> cachedValue = null;
            try
            {
                cachedValue = Cache.Get<T>(Key, Region);

                CacheValidationResult validationResult = ValidateCachedValue(cachedValue);

                if (validationResult != CacheValidationResult.Valid && (validationResult == CacheValidationResult.Invalid || cachedValue == null))
                    cachedValue = RetrieveCachedValue(previousCachedValue: cachedValue);

                return cachedValue;
            }
            catch (FluentCache.FluentCacheException cacheException)
            {
                if (!TryHandleCachingFailure(cacheException))
                    throw;
                else
                    return null;
            }
        }

        /// <summary>
        /// Asynchronously executes the plan
        /// </summary>
        public async Task<CachedValue<T>> ExecuteAsync()
        {
            CachedValue<T> cachedValue = null;
            try
            {
                cachedValue = Cache.Get<T>(Key, Region);

                CacheValidationResult validationResult = await ValidateCachedValueAsync(cachedValue);

                if (validationResult != CacheValidationResult.Valid && (validationResult == CacheValidationResult.Invalid || cachedValue == null))
                    cachedValue = await RetrieveCachedValueAsync(previousCachedValue: cachedValue);

                return cachedValue;
            }
            catch (FluentCache.FluentCacheException cacheException)
            {
                if (!TryHandleCachingFailure(cacheException))
                    throw;
                else
                    return null;
            }
        }
    }

}
