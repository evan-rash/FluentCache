using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A cache for a particular source whose methods you want to cache. Various Fluent extension methods simplify the process
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public sealed class Cache<TSource> : Cache
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public Cache(TSource source, Cache cache)
        {
            _source = source;
            _cache = cache;
        }
        private readonly Cache _cache;
        private readonly TSource _source;

        /// <summary>
        /// Gets the source whose methods you want to cache
        /// </summary>
        public TSource Source { get { return _source; } }

        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key of the value to retrieve</param>
        /// <param name="region">The region in the cache where the key is stored</param>
        /// <returns>The cached value</returns>
        public override CachedValue<T> Get<T>(string key, string region)
        {
            return _cache.Get<T>(key, region);
        }

        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key of the value to retrieve</param>
        /// <param name="region">The region in the cache where the key is stored</param>
        /// <param name="value">The cached value</param>
        /// <param name="cacheExpiration">The expiration policy for this value</param>
        /// <returns>The cached value</returns>
        public override CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration)
        {
            return _cache.Set(key, region, value, cacheExpiration);
        }

        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        public override void Remove(string key, string region)
        {
            _cache.Remove(key, region);
        }

        /// <summary>
        /// Combines the key and region to generate a unique cache key
        /// </summary>
        protected internal override string GetCacheKey(string key, string region)
        {
            return _cache.GetCacheKey(key, region);
        }

        /// <summary>
        /// Gets a string representation of a parameter value that is used to build up a unique cache key for a parametized caching expression. The default implementation is parameterValue.ToString()
        /// </summary>
        protected internal override string GetParameterCacheKeyValue(object parameterValue)
        {
            return _cache.GetParameterCacheKeyValue(parameterValue);
        }

        /// <summary>
        /// Marks a value in the cache as validated
        /// </summary>
        protected internal override void MarkAsValidated(string key, string region)
        {
            _cache.MarkAsValidated(key, region);
        }
    }
}
