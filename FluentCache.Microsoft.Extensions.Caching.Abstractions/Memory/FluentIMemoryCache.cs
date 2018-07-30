using System;
using System.Collections.Generic;
using System.Text;
using FluentCache.Execution;
using Microsoft.Extensions.Caching.Memory;

namespace FluentCache.Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// Provides a FluentCache.ICache wrapper around Microsoft.Extensions.Caching.Memory.IMemoryCache
    /// </summary>
    public class FluentIMemoryCache : ICache
    {
        /// <summary>
        /// Constructs a FluentMemoryCache wrapper around IMemoryCache
        /// </summary>
        public FluentIMemoryCache(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        private readonly IMemoryCache MemoryCache;

        
        /// <summary>
        /// Gets the specified cached value
        /// </summary>
        public CachedValue<T> Get<T>(string key, string region)
        {
            string k = GetCacheKey(key, region);
            MemoryStorage storage = MemoryCache.Get(k) as MemoryStorage;
            return storage?.ToCachedValue<T>();
        }

        /// <summary>
        /// Sets the specified cached value
        /// </summary>
        public CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration)
        {
            DateTime now = DateTime.UtcNow;
            string k = GetCacheKey(key, region);
            if (MemoryCache.Get(k) is MemoryStorage storage)
            {
                storage.Version++;
                storage.LastValidatedDate = now;
                storage.CacheDate = now;
                storage.Value = value;
            }
            else
            {
                storage = new MemoryStorage
                {
                    CacheDate = now,
                    LastValidatedDate = now,
                    Version = 0L,
                    Value = value
                };

                var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheExpiration?.SlidingExpiration };
                MemoryCache.Set(k, storage, options);
            }

            return storage.ToCachedValue<T>();
        }

        /// <summary>
        /// Removes the specified cached value
        /// </summary>
        public void Remove(string key, string region)
        {
            string k = GetCacheKey(key, region);
            MemoryCache.Remove(k);
        }
       
        /// <summary>
        /// Marks the specified cached value as validated
        /// </summary>
        public void MarkAsValidated(string key, string region)
        {
            var now = DateTime.UtcNow;
            string k = GetCacheKey(key, region);
            if (MemoryCache.Get(k) is MemoryStorage storage)
                storage.LastValidatedDate = now;
        }

        /// <summary>
        /// Generates a unique key for the parameter value
        /// </summary>
        public virtual string GetParameterCacheKeyValue(object parameterValue)
        {
            return ParameterCacheKeys.GenerateCacheKey(parameterValue);
        }

        /// <summary>
        /// Creates an execution plan for retrieving a cached value
        /// </summary>
        public virtual ICacheExecutionPlan<T> CreateExecutionPlan<T>(ICacheStrategy<T> cacheStrategy)
        {
            return new CacheExecutionPlan<T>(this, cacheStrategy);
        }

        private string GetCacheKey(string key, string region)
        {
            return region + ":" + key;
        }
    }
}
