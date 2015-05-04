using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Provides methods for consumers to cache and retrieve data
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <param name="key">A key identifying the value that is to be retrieved</param>
        /// <param name="region">A region used to group related data elements</param>
        /// <returns>The cached value</returns>
        ICachedValue Get(string key, string region);
        
        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <param name="key">A key identifying the value that is to be retrieved</param>
        /// <param name="region">A region used to group related data elements</param>
        /// <returns>The cached value</returns>
        ICachedValue<T> Get<T>(string key, string region);

        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        /// <param name="key">A key identifying the value that is to be retrieved</param>
        /// <param name="region">A region used to group related data elements</param>
        /// <param name="value">The value to cache</param>
        /// <param name="cachePolicy">A policy that specifies how the value should be cached</param>
        /// <returns></returns>
        ICachedValue Set(string key, string region, object value, CachePolicy cachePolicy);

        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        /// <param name="key">A key identifying the value that is to be retrieved</param>
        /// <param name="region">A region used to group related data elements</param>
        /// <param name="value">The value to cache</param>
        /// <param name="cachePolicy">A policy that specifies how the value should be cached</param>
        /// <returns></returns>
        ICachedValue<T> Set<T>(string key, string region, T value, CachePolicy cachePolicy);
        
        /// <summary>
        /// Removes a value from the cache
        /// </summary>
        /// <param name="key">A key identifying the value that is to be retrieved</param>
        /// <param name="region">A region used to group related data elements</param>
        void Remove(string key, string region);

        /// <summary>
        /// Determines how cache keys should be generated for parameter values
        /// </summary>
        IParameterCacheKeyProvider ParameterCacheKeyProvider { get; }

        /// <summary>
        /// Attempts to handle an error while attempting to get, set, or remove an item from the cache.
        /// Returning true indicates that the exception is handled, false indicates the exception is unhandled and should be re-thrown
        /// </summary>
        /// <param name="exception">The exception that was encountered</param>
        /// <param name="cacheOperation">The operation where the failure occured</param>
        /// <returns>True to indicate the exception has been handled, false to indicate the exception is unhandled and should be re-thrown</returns>
        bool TryHandleCachingFailure(FluentCacheException exception, CacheOperation cacheOperation);
    }

}