using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Strategies
{
    /// <summary>
    /// A strategy to access or update a single value in a cache
    /// </summary>
    public class SingleValueCacheStrategy : CacheStrategy
    {
        internal SingleValueCacheStrategy(Cache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        /// <summary>
        /// Clears the value associated with this caching strategy from the cache
        /// </summary>
        public void ClearValue()
        {
            Cache.Remove(Key, Region);
        }

    }
}
