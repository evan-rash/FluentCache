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
    public interface ICache
    {
        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key of the value to retrieve</param>
        /// <param name="region">The region in the cache where the key is stored</param>
        /// <returns>The cached value</returns>
        CachedValue<T> Get<T>(string key, string region);

        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key of the value to retrieve</param>
        /// <param name="region">The region in the cache where the key is stored</param>
        /// <param name="value">The cached value</param>
        /// <param name="cacheExpiration">The expiration policy for this value</param>
        /// <returns>The cached value</returns>
        CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration);

        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        void Remove(string key, string region);

        /// <summary>
        /// Marks a value in the cache as validated
        /// </summary>
        void MarkAsValidated(string key, string region);

        /// <summary>
        /// Gets a string representation of a parameter value that is used to build up a unique cache key for a parametized caching expression
        /// </summary>
        string GetParameterCacheKeyValue(object parameterValue);

        /// <summary>
        /// Creates an execution plan for the specified caching strategy
        /// </summary>
        Execution.ICacheExecutionPlan<T> CreateExecutionPlan<T>(ICacheStrategy<T> cacheStrategy);
    }


}