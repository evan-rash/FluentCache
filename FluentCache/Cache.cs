using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Represents a cache that can be used to store and retrieve values
    /// </summary>
    public abstract class Cache
    {
        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key of the value to retrieve</param>
        /// <param name="region">The region in the cache where the key is stored</param>
        /// <returns>The cached value</returns>
        public abstract CachedValue<T> Get<T>(string key, string region);

        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key of the value to retrieve</param>
        /// <param name="region">The region in the cache where the key is stored</param>
        /// <param name="value">The cached value</param>
        /// <param name="cacheExpiration">The expiration policy for this value</param>
        /// <returns>The cached value</returns>
        public abstract CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration);

        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        public abstract void Remove(string key, string region);

        /// <summary>
        /// Marks a value in the cache as validated
        /// </summary>
        protected internal abstract void MarkAsValidated(string key, string region);

        /// <summary>
        /// Gets a string representation of a parameter value that is used to build up a unique cache key for a parametized caching expression. The default implementation is parameterValue.ToString()
        /// </summary>
        protected internal virtual string GetParameterCacheKeyValue(object parameterValue)
        {
            if (parameterValue == null)
                return String.Empty;
            else
                return parameterValue.ToString();
        }

        /// <summary>
        /// Combines the key and region to generate a unique cache key
        /// </summary>
        protected internal virtual string GetCacheKey(string key, string region)
        {
            return region + "." + key;
        }

        /// <summary>
        /// Creates an execution plan for the specified caching strategy
        /// </summary>
        public virtual CacheExecutionPlan<T> CreateExecutionPlan<T>(ICacheStrategy<T> cacheStrategy)
        {
            return new CacheExecutionPlan<T>(this, cacheStrategy);
        }
    }


}