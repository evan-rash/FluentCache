using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Specifies a caching strategy to retrieve cached values asynchronously
    /// </summary>
    public interface ICacheStrategyAsync<T> : ICacheStrategy<T>
    {
        /// <summary>
        /// Retrieves the cached value asynchronously
        /// </summary>
        Task<T> RetrieveAsync(CachedValue<T> existingCachedValue);

        /// <summary>
        /// Validates the cached value asynchronously
        /// </summary>
        Task<CacheValidationResult> ValidateAsync(CachedValue<T> existingCachedValue);
    }
}