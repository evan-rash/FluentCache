using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Strategies
{
    /// <summary>
    /// A strategy to access or modify strongly-typed data in a Cache that depends on one or more parameters
    /// </summary>
    public sealed class CacheStrategyParameterized<P1> : CacheStrategyIncomplete
    {
        internal CacheStrategyParameterized(Cache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategy<T> RetrieveUsing<T>(Func<P1, T> retrieve)
        {
            CacheStrategy<T> strategy = base.Complete<T>();
            var p1 = strategy.GetParameter<P1>(0);

            strategy.RetrieveCallback = () => retrieve(p1);

            return strategy;
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">An asynchronous delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategyAsync<T> RetrieveUsingAsync<T>(Func<P1, Task<T>> retrieve)
        {
            CacheStrategyAsync<T> strategy = base.CompleteAsync<T>();
            var p1 = strategy.GetParameter<P1>(0);

            strategy.RetrieveCallback = () => retrieve(p1);

            return strategy;
        }
    }

    /// <summary>
    /// A strategy to access or modify strongly-typed data in a Cache that depends on one or more parameters
    /// </summary>
    public sealed class CacheStrategyParameterized<P1, P2> : CacheStrategyIncomplete
    {
        internal CacheStrategyParameterized(Cache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategy<T> Retrieve<T>(Func<P1,P2, T> retrieve)
        {
            CacheStrategy<T> strategy = base.Complete<T>();
            var p1 = strategy.GetParameter<P1>(0);
            var p2 = strategy.GetParameter<P2>(1);


            strategy.RetrieveCallback = () => retrieve(p1, p2);

            return strategy;
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">An asynchronous delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategyAsync<T> RetrieveAsync<T>(Func<P1, P2, Task<T>> retrieve)
        {
            CacheStrategyAsync<T> strategy = base.CompleteAsync<T>();
            var p1 = strategy.GetParameter<P1>(0);
            var p2 = strategy.GetParameter<P2>(1);

            strategy.RetrieveCallback = () => retrieve(p1, p2);

            return strategy;
        }
    }

    /// <summary>
    /// A strategy to access or modify strongly-typed data in a Cache that depends on one or more parameters
    /// </summary>
    public sealed class CacheStrategyParameterized<P1, P2, P3> : CacheStrategyIncomplete
    {
        internal CacheStrategyParameterized(Cache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategy<T> RetrieveUsing<T>(Func<P1, P2,P3, T> retrieve)
        {
            CacheStrategy<T> strategy = base.Complete<T>();
            var p1 = strategy.GetParameter<P1>(0);
            var p2 = strategy.GetParameter<P2>(1);
            var p3 = strategy.GetParameter<P3>(2);


            strategy.RetrieveCallback = () => retrieve(p1, p2, p3);

            return strategy;
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">An asynchronous delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategyAsync<T> RetrieveUsingAsync<T>(Func<P1, P2, P3, Task<T>> retrieve)
        {
            CacheStrategyAsync<T> strategy = base.CompleteAsync<T>();
            var p1 = strategy.GetParameter<P1>(0);
            var p2 = strategy.GetParameter<P2>(1);
            var p3 = strategy.GetParameter<P3>(2);

            strategy.RetrieveCallback = () => retrieve(p1, p2, p3);

            return strategy;
        }
    }

    /// <summary>
    /// A strategy to access or modify strongly-typed data in a Cache that depends on one or more parameters
    /// </summary>
    public sealed class CacheStrategyParameterized<P1, P2, P3, P4> : CacheStrategyIncomplete
    {
        internal CacheStrategyParameterized(Cache cache, string baseKey)
            : base(cache, baseKey)
        {
        }


        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategy<T> RetrieveUsing<T>(Func<P1, P2, P3, P4, T> retrieve)
        {
            CacheStrategy<T> strategy = base.Complete<T>();
            var p1 = strategy.GetParameter<P1>(0);
            var p2 = strategy.GetParameter<P2>(1);
            var p3 = strategy.GetParameter<P3>(2);
            var p4 = strategy.GetParameter<P4>(3);

            strategy.RetrieveCallback = () => retrieve(p1, p2, p3, p4);

            return strategy;
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">An asynchronous delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategyAsync<T> RetrieveUsingAsync<T>(Func<P1, P2, P3, P4,  Task<T>> retrieve)
        {
            CacheStrategyAsync<T> strategy = base.CompleteAsync<T>();
            var p1 = strategy.GetParameter<P1>(0);
            var p2 = strategy.GetParameter<P2>(1);
            var p3 = strategy.GetParameter<P3>(2);
            var p4 = strategy.GetParameter<P4>(3);

            strategy.RetrieveCallback = () => retrieve(p1, p2, p3, p4);

            return strategy;
        }
    }
}
