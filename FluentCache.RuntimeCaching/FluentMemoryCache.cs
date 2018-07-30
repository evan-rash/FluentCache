using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.RuntimeCaching
{
    /// <summary>
    /// Provides a FluentCache.ICache wrapper around the System.Runtime.Caching.MemoryCache
    /// </summary>
    public class FluentMemoryCache : ICache
    {
        /// <summary>
        /// Creates a new FluentMemoryCache wrapper around MemoryCache.Default
        /// </summary>
        /// <returns></returns>
        public static ICache Default()
        {
            return new FluentMemoryCache(MemoryCache.Default);
        }

        /// <summary>
        /// Creates a new FluentMemoryCache wrapper around the specified MemoryCache
        /// </summary>
        public FluentMemoryCache(MemoryCache memoryCache)
        {
            MemoryCache = memoryCache ?? throw new ArgumentNullException("memoryCache");
        }

        private readonly MemoryCache MemoryCache;

        private class Storage
        {
            public DateTime CacheDate { get; set; }
            public DateTime LastValidatedDate { get; set; }
            public long Version { get; set; }
            public object Value { get; set; }

            public CachedValue<T> ToCachedValue<T>()
            {
                if (!(Value is T))
                    return null;

                return new CachedValue<T>
                {
                    CachedDate = CacheDate,
                    LastValidatedDate = LastValidatedDate,
                    Value = (T)Value,
                    Version = Version
                };
            }

        }

        /// <summary>
        /// Gets the specified cached value
        /// </summary>
        public CachedValue<T> Get<T>(string key, string region)
        {
            string k = GetCacheKey(key, region);
            Storage storage = MemoryCache.Get(k) as Storage;
            if (storage == null)
                return null;

            return storage.ToCachedValue<T>();
        }

        /// <summary>
        /// Sets the specified cached value
        /// </summary>
        public CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration)
        {
            DateTime now = DateTime.UtcNow;
            string k = GetCacheKey(key, region);
            if (MemoryCache.Get(k) is Storage storage)
            {
                storage.Version++;
                storage.LastValidatedDate = now;
                storage.CacheDate = now;
                storage.Value = value;
            }
            else
            {
                storage = new Storage
                {
                    CacheDate = now,
                    LastValidatedDate = now,
                    Value = value,
                    Version = 0L
                };

                var cachePolicy = new CacheItemPolicy();
                if (cacheExpiration?.SlidingExpiration != null)
                    cachePolicy.SlidingExpiration = cacheExpiration.SlidingExpiration.GetValueOrDefault();

                MemoryCache.Add(k, storage, cachePolicy);
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
        /// Marks the specified cached value as modified
        /// </summary>
        public void MarkAsValidated(string key, string region)
        {
            DateTime now = DateTime.UtcNow;
            string k = GetCacheKey(key, region);
            if (MemoryCache.Get(k) is Storage storage)
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
        public virtual Execution.ICacheExecutionPlan<T> CreateExecutionPlan<T>(ICacheStrategy<T> cacheStrategy)
        {
            return new Execution.CacheExecutionPlan<T>(this, cacheStrategy);
        }

        private string GetCacheKey(string key, string region)
        {
            return region + ":" + key;
        }
    }
}
