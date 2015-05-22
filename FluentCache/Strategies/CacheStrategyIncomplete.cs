using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Strategies
{
    /// <summary>
    /// An incomplete strategy to access or modify strongly-typed data in a Cache
    /// </summary>
    public class CacheStrategyIncomplete : SingleValueCacheStrategy
    {
        internal CacheStrategyIncomplete(Cache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        [Obsolete("Deprecated. Use RetrieveUsing() instead")]
        public CacheStrategy<T> Retrieve<T>(Func<T> retrieve)
        {
            return Complete<T>().RetrieveUsing(retrieve);
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategy<T> RetrieveUsing<T>(Func<T> retrieve)
        {
            return Complete<T>().RetrieveUsing(retrieve);
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">An asynchronous delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategyAsync<T> RetrieveUsingAsync<T>(Func<Task<T>> retrieve)
        {
            return CompleteAsync<T>().RetrieveUsingAsync(retrieve);
        }

        /// <summary>
        /// Gets the cached value wrapper from the cache
        /// </summary>
        /// <returns>The cached value wrapper</returns>
        public CachedValue<T> Get<T>()
        {
            return Complete<T>().Get();
        }

        /// <summary>
        /// Gets the cached value
        /// </summary>
        /// <returns>The cached value</returns>
        public T GetValue<T>()
        {
            CacheStrategy<T> copy = Complete<T>();
            return copy.GetValue();
        }

        internal CacheStrategy<T> Complete<T>()
        {
            var copy = new CacheStrategy<T>(Cache, BaseKey);
            copy.CopyFrom(this);
            return copy;
        }

        internal CacheStrategyAsync<T> CompleteAsync<T>()
        {
            var copy = new CacheStrategyAsync<T>(Cache, BaseKey);
            copy.CopyFrom(this);
            return copy;
        }

        /// <summary>
        /// Updates the cache strategy to include strongly typed parameters
        /// </summary>
        public CacheStrategyParameterized<P1> WithParameters<P1>(P1 param1)
        {
            var copy = new CacheStrategyParameterized<P1>(Cache, BaseKey);
            copy.CopyFrom(this);
            copy.Parameters = new List<object> { param1 };
            return copy;
        }

        /// <summary>
        /// Updates the cache strategy to include strongly typed parameters
        /// </summary>
        public CacheStrategyParameterized<P1, P2> WithParameters<P1, P2>(P1 param1, P2 param2)
        {
            var copy = new CacheStrategyParameterized<P1, P2>(Cache, BaseKey);
            copy.CopyFrom(this);
            copy.Parameters = new List<object> { param1, param2 };
            return copy;
        }

        /// <summary>
        /// Updates the cache strategy to include strongly typed parameters
        /// </summary>
        public CacheStrategyParameterized<P1, P2, P3> WithParameters<P1, P2, P3>(P1 param1, P2 param2, P3 param3)
        {
            var copy = new CacheStrategyParameterized<P1, P2, P3>(Cache, BaseKey);
            copy.CopyFrom(this);
            copy.Parameters = new List<object> { param1, param2, param3 };
            return copy;
        }

        /// <summary>
        /// Updates the cache strategy to include strongly typed parameters
        /// </summary>
        public CacheStrategyParameterized<P1, P2, P3, P4> WithParameters<P1, P2, P3, P4>(P1 param1, P2 param2, P3 param3, P4 param4)
        {
            var copy = new CacheStrategyParameterized<P1, P2, P3, P4>(Cache, BaseKey);
            copy.CopyFrom(this);
            copy.Parameters = new List<object> { param1, param2, param3, param4 };
            return copy;
        }

        /// <summary>
        /// Updates the cache strategy to include parameters
        /// </summary>
        public CacheStrategyIncomplete WithParameters(params object[] parameters)
        {
            this.Parameters = parameters.ToList();
            return this;
        }


    }

}
