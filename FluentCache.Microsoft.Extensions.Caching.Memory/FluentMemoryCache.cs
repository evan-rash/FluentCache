using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace FluentCache.Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// Provides a FluentCache.ICache wrapper around Microsoft.Extensions.Caching.Memory.MemoryCache
    /// </summary>
    public class FluentMemoryCache : FluentIMemoryCache
    {
        private static MemoryCache CreateCache(MemoryCacheOptions options = null)
        {
            return new MemoryCache(options ?? new MemoryCacheOptions());
        }

        /// <summary>
        /// Construct a new FluentMemoryCache using the default MemoryCache options
        /// </summary>
        public FluentMemoryCache()
            : this(CreateCache())
        {

        }

        /// <summary>
        /// Construct a new FluentMemoryCache using the specified memory cache options
        /// </summary>
        public FluentMemoryCache(MemoryCacheOptions options)
            : this(CreateCache(options))
        {

        }


        /// <summary>
        /// Construct a new FluentMemoryCache from the specified MemoryCache
        /// </summary>
        public FluentMemoryCache(MemoryCache memoryCache) : base(memoryCache)
        {
        }
    }
}
