using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Simple
{
    /// <summary>
    /// A simple Cache implementation that uses Dictionary to cache values
    /// </summary>
    public class FluentDictionaryCache : Cache
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public FluentDictionaryCache()
        {
            Dictionary = new ConcurrentDictionary<string, Storage>();
        }

        private readonly ConcurrentDictionary<string, Storage> Dictionary;

        /// <summary>
        /// Gets the specified cached value
        /// </summary>
        public override CachedValue<T> Get<T>(string key, string region)
        {
            string k = GetCacheKey(key, region);
            Storage storage;
            if (!Dictionary.TryGetValue(k, out storage))
                return null;

            DateTime now = DateTime.UtcNow;
            if (now - storage.LastAccessedDate > storage.Expiration.SlidingExpiration)
            {
                Dictionary.TryRemove(k, out storage);
                return null;
            }

            storage.LastAccessedDate = now;

            return storage.ToCachedValue<T>();
        }

        /// <summary>
        /// Sets the specified cached value
        /// </summary>
        public override CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration)
        {
            DateTime now = DateTime.UtcNow;
            string k = GetCacheKey(key, region);
            
            var newStorage = new Storage
            {
                CacheDate = now,
                LastAccessedDate = now,
                LastValidatedDate = now,
                Expiration = cacheExpiration,
                Value = value,
                Version = 0L,
            };

            Func<string, Storage, Storage> updateIfExists = (newKey, existing) =>
                {
                    existing.Version = existing.Version + 1L;
                    existing.LastValidatedDate = now;
                    existing.LastAccessedDate = now;
                    existing.Value = value;
                    return existing;
                };

            Storage storage = Dictionary.AddOrUpdate(k, newStorage, updateIfExists);
            return storage.ToCachedValue<T>();
        }

        /// <summary>
        /// Removes the specified cached value
        /// </summary>
        public override void Remove(string key, string region)
        {
            string k = GetCacheKey(key, region);
            Storage storage;
            Dictionary.TryRemove(k, out storage);
        }

        /// <summary>
        /// Marks the specified cached value as validated
        /// </summary>
        protected internal override void MarkAsValidated(string key, string region)
        {
            DateTime now = DateTime.UtcNow;
            Func<string, Storage, Storage> updateLastModifiedDate = (newKey, existing) =>
                {
                    if (existing == null)
                        return null;

                    existing.LastValidatedDate = now;
                    return existing;
                };

            //Note: if the caller tries to mark validated for a non-existing item then we will just insert a null Storage object
            Dictionary.AddOrUpdate(key, default(Storage), updateLastModifiedDate);
        }

        private class Storage
        {
            public DateTime CacheDate { get; set; }
            public long Version { get; set; }
            public DateTime LastValidatedDate { get; set; }
            public DateTime LastAccessedDate { get; set; }
            public object Value { get; set; }
            public CacheExpiration Expiration { get; set; }

            public CachedValue<T> ToCachedValue<T>()
            {
                return new CachedValue<T>
                {
                    CachedDate = CacheDate,
                    LastValidatedDate = LastValidatedDate,
                    Value = (T)Value,
                    Version = Version
                };
            }
        }
    }
}
