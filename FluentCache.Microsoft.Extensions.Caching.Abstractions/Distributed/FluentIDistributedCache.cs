using System;
using System.Collections.Generic;
using System.Text;
using FluentCache.Execution;
using Microsoft.Extensions.Caching.Distributed;

namespace FluentCache.Microsoft.Extensions.Caching.Distributed
{
    /// <summary>
    /// Provides a FluentCache.ICache wrapper around the Microsoft.Extensions.Caching.Distributed.IDistributedCache
    /// </summary>
    public class FluentIDistributedCache : ICache
    {
        /// <summary>
        /// Constructs a new FluentDistributedCache wrapper around IDistributedCache
        /// </summary>
        public FluentIDistributedCache(IDistributedCache distributedCache, ISerializer serializer)
        {
            DistributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        private readonly IDistributedCache DistributedCache;
        private readonly ISerializer Serializer;

        /// <summary>
        /// Gets the specified cached value
        /// </summary>
        public CachedValue<T> Get<T>(string key, string region)
        {
            string k = GetCacheKey(key, region);
            byte[] bytes = DistributedCache.Get(k);
            if (bytes == null)
                return null;

            var storage = DistributedStorage.FromBytes(Serializer, bytes);
            return storage.ToCachedValue<T>(Serializer);
        }

        /// <summary>
        /// Sets the specified cached value
        /// </summary>

        public CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration)
        {
            var now = DateTime.UtcNow;
            string k = GetCacheKey(key, region);

            DistributedStorage storage = null;
            string serializedValue = Serializer.Serialize(value);
            byte[] bytes = DistributedCache.Get(k);

            if (bytes == null)
            {
                storage = new DistributedStorage
                {
                    CacheDate = now,
                    Version = 0L,
                    Value = serializedValue,
                    LastValidatedDate = now
                };
            }
            else
            {
                storage = DistributedStorage.FromBytes(Serializer, bytes);
                storage.Version++;
                storage.LastValidatedDate = now;
                storage.Value = serializedValue;
            }

            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheExpiration?.SlidingExpiration };
            DistributedCache.Set(k, storage.ToBytes(Serializer), options);

            return storage.ToCachedValue<T>(Serializer);
        }

        /// <summary>
        /// Removes the specified cached value
        /// </summary>
        public void Remove(string key, string region)
        {
            string k = GetCacheKey(key, region);
            DistributedCache.Remove(k);
        }

        /// <summary>
        /// Marks the specified cached value as modified
        /// </summary>
        public void MarkAsValidated(string key, string region)
        {
            var now = DateTime.UtcNow;
            string k = GetCacheKey(key, region);
            byte[] bytes = DistributedCache.Get(k);
            if (bytes == null)
                return;

            var storage = DistributedStorage.FromBytes(Serializer, bytes);
            storage.LastValidatedDate = now;
            DistributedCache.Set(k, bytes);
        }


        private string GetCacheKey(string key, string region)
        {
            return region + ":" + key;
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
    }
}
