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
    public struct CacheExpiration
    {
        ///// <summary>
        ///// Creates a cache policy with the specified sliding expiration and expiration date
        ///// </summary>
        //public CachePolicy(TimeSpan? slidingExpiration, DateTime? expirationDate)
        //{
        //    _SlidingExpiration = slidingExpiration;
        //    _ExpirationDate = expirationDate;
        //}

        /// <summary>
        /// Creates a cache policy with the specified sliding expiration
        /// </summary>
        public CacheExpiration(TimeSpan? slidingExpiration)
        {
            _SlidingExpiration = slidingExpiration;
            //_ExpirationDate = null;
        }

        ///// <summary>
        ///// Creates a cache policy with the specified expiration date
        ///// </summary>
        //public CachePolicy(DateTime? expirationDate)
        //{
        //    _ExpirationDate = expirationDate;
        //    _SlidingExpiration = null;
        //}

        private readonly TimeSpan? _SlidingExpiration;
        //private readonly DateTime? _ExpirationDate;

        /// <summary>
        /// Returns a new caching policy with the specified sliding expiration period
        /// </summary>
        public CacheExpiration ExpireAfter(TimeSpan timeSpan)
        {
            return new CacheExpiration(timeSpan);
        }

        ///// <summary>
        ///// Returns a new cache policy with the specified expiration date
        ///// </summary>
        //public CachePolicy ExpireOn(DateTime expireOn)
        //{
        //    if (expireOn.Kind != DateTimeKind.Utc)
        //        throw new ArgumentException("expireOn must be a UTC date", "expireOn");
        //    return new CachePolicy(this.SlidingExpiration, expireOn);
        //}

        /// <summary>
        /// Gets the sliding expiration duration for a cached item
        /// </summary>
        public TimeSpan? SlidingExpiration { get { return _SlidingExpiration; } }
        
        ///// <summary>
        ///// Gets the absolute expiration date for a cached item
        ///// </summary>
        //public DateTime? ExpirationDate { get { return _ExpirationDate; } }
    }
}
