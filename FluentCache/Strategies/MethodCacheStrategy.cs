using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Strategies
{
    /// <summary>
    /// Defines a caching strategy for a particular method
    /// </summary>
    internal class MethodCacheStrategy<TResult> : CacheStrategyIncomplete
    {
        internal MethodCacheStrategy(ICache cache, Func<TResult> method, string baseKey)
            : base(cache, baseKey)
        {
            Method = method;
        }

        private readonly Func<TResult> Method;

        /// <summary>
        /// Indicates that the method used to create the caching strategy should also be used to retrieve the value if it is missing or invalid
        /// </summary>
        /// <returns>A caching strategy that can be used to access the value</returns>
        public CacheStrategy<TResult> RetrieveUsingMethod()
        {
            return this.RetrieveUsing(Method);
        }

        /// <summary>
        /// Sets the cached value
        /// </summary>
        /// <param name="value">The value to be cached</param>
        public void SetValue(TResult value)
        {
            Cache.Set(Key, Region, value, ResolveExpiration(value));
        }

        /// <summary>
        /// Gets the cached value
        /// </summary>
        /// <returns>The cached value</returns>
        public TResult GetValue()
        {
            return base.GetValue<TResult>();
        }
    }

    /// <summary>
    /// Defines an asynchronous caching strategy for a particular method
    /// </summary>
    public class AsyncMethodCacheStrategy<TResult> : CacheStrategyIncomplete
    {
        internal AsyncMethodCacheStrategy(ICache cache, Func<Task<TResult>> method, string baseKey)
            : base(cache, baseKey)
        {
            Method = method;
        }

        private readonly Func<Task<TResult>> Method;

        /// <summary>
        /// Indicates that the method used to create the caching strategy should also be used to retrieve the value if it is missing or invalid
        /// </summary>
        /// <returns>A caching strategy that can be used to access the value</returns>
        public CacheStrategyAsync<TResult> RetrieveUsingMethod()
        {
            return this.RetrieveUsingAsync(Method);
        }

        /// <summary>
        /// Sets the cached value
        /// </summary>
        /// <param name="value">The value to be cached</param>
        public void SetValue(TResult value)
        {
            Cache.Set(Key, Region, value, ResolveExpiration(value));
        }

        /// <summary>
        /// Gets the cached value
        /// </summary>
        /// <returns>The cached value</returns>
        public TResult GetValue()
        {
            return base.GetValue<TResult>();
        }
    }
}
