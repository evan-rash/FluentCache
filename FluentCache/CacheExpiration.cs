using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Defines a policy for how values should be expired
    /// </summary>
    public class CacheExpiration
    {

        /// <summary>
        /// Creates a new default cache policy with no expiration
        /// </summary>
        public CacheExpiration()
            : this(null)
        {

        }

        /// <summary>
        /// Creates a cache policy with the specified sliding expiration
        /// </summary>
        public CacheExpiration(TimeSpan? slidingExpiration)
        {
            _slidingExpiration = slidingExpiration;
        }

        private readonly TimeSpan? _slidingExpiration;

        /// <summary>
        /// Resolves the sliding expiration duration for a cached item
        /// </summary>
        public TimeSpan? SlidingExpiration => _slidingExpiration;

        
    }
}
