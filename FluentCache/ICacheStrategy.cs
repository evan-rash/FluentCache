using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Specifies a caching strategy that is used by a Cache
    /// </summary>
    public interface ICacheStrategy<T>
    {
        /// <summary>
        /// Gets the key of this cached value. Should be unique in combination with Region
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the cache region. Should be unique in combination with Key
        /// </summary>
        string Region { get; }

        /// <summary>
        /// Resolves the expiration policy for the value that will be cached
        /// </summary>
        CacheExpiration ResolveExpiration(T value);

        /// <summary>
        /// Validates an existing cached value
        /// </summary>
        CacheValidationResult Validate(CachedValue<T> existingCachedValue);

        /// <summary>
        /// Retrieves a cached value
        /// </summary>
        T Retrieve(CachedValue<T> existingCachedValue);
    }
}
