using FluentCache.Expressions;
using FluentCache.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Fluent Cache Extensions
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Gets the specified value from a cache, or the default value if no matching item was found in the cache
        /// </summary>
        /// <typeparam name="T">The cached item type</typeparam>
        /// <param name="source">the cache</param>
        /// <param name="key">the key of the cached item</param>
        /// <param name="region">the region for the cache</param>
        /// <returns>The cached value, or default(T) if the item is not in the cache</returns>
        public static T GetValue<T>(this ICache source, string key, string region)
        {
            CachedValue<T> cache = source.Get<T>(key, region);
            return cache == null ? default(T) : cache.Value;
        }

        /// <summary>
        /// Returns a caching strategy for the calling method. The method name will be used in the cache key
        /// </summary>
        public static CacheStrategyIncomplete ThisMethod(this ICache source, [CallerMemberName]string method = null)
        {
            return new CacheStrategyIncomplete(source, method);
        }

        /// <summary>
        /// Returns a caching strategy for the calling method. The method name and source type will be used in the cache key
        /// </summary>
        /// <typeparam name="TSource">The type of the caching source</typeparam>
        /// <param name="source">The caching source</param>
        /// <param name="method">The name of the method that will be cached</param>
        /// <returns></returns>
        public static CacheStrategyIncomplete ThisMethod<TSource>(this Cache<TSource> source, [CallerMemberName]string method = null)
        {
            return new CacheStrategyIncomplete(source, method)
                            .WithRegion(typeof(TSource).Name);
        }

        /// <summary>
        /// Returns a cache wrapper that can support caching strategies for member expressions
        /// ex: cache.WithSource(this).Method(t => t.MyMethod())
        /// </summary>
        public static Cache<T> WithSource<T>(this ICache source, T caller)
        {
            return new Cache<T>(caller, source);
        }

        /// <summary>
        /// Returns a caching strategy with the specified key
        /// </summary>
        public static CacheStrategyIncomplete WithKey(this ICache source, string key)
        {
            return new CacheStrategyIncomplete(source, key);
        }

        /// <summary>
        /// Updates a caching strategy to use the specified region
        /// </summary>
        public static TPlan WithRegion<TPlan>(this TPlan source, string region) where TPlan : CacheStrategy
        {
            source.Region = region;
            return source;
        }

        /// <summary>
        /// Updates a caching strategy to use the specified sliding absolute expiration date
        /// </summary>
        public static TPlan ExpireAfter<TPlan>(this TPlan source, TimeSpan expireAfter) where TPlan : CacheStrategy
        {
            source.Expiration = source.Expiration.ExpireAfter(expireAfter);
            return source;

        }

        /// <summary>
        /// Validates the item was cached after the specified date
        /// </summary>
        public static CacheStrategy<T> ValidateCachedAfter<T>(this CacheStrategy<T> source, DateTime afterDate)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            else if (afterDate.Kind != DateTimeKind.Utc)
                throw new ArgumentException("afterDate must be in UTC", "afterDate");

            return source.InvalidateIf(c => c.CachedDate > afterDate);
        }

        /// <summary>
        /// Validates the item was cached after the specified date
        /// </summary>
        public static CacheStrategyAsync<T> ValidateCachedAfter<T>(this CacheStrategyAsync<T> source, DateTime afterDate)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            else if (afterDate.Kind != DateTimeKind.Utc)
                throw new ArgumentException("afterDate must be in UTC", "afterDate");

            return source.InvalidateIf(c => c.CachedDate > afterDate);
        }

        ///// <summary>
        ///// Validates all cached items were cached after the specified date
        ///// </summary>
        //public static BulkCacheStrategy<TKey, TResult> ValidateCachedAfter<TKey, TResult>(this BulkCacheStrategy<TKey, TResult> source, DateTime afterDate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");
        //    else if (afterDate.Kind != DateTimeKind.Utc)
        //        throw new ArgumentException("afterDate must be in UTC", "afterDate");

        //    return source.InvalidateIf(c => c.CachedDate > afterDate);
        //}

        ///// <summary>
        ///// Validates all cached items were cached after the specified date
        ///// </summary>
        //public static BulkCacheStrategyAsync<TKey, TResult> ValidateCachedAfter<TKey, TResult>(this BulkCacheStrategyAsync<TKey, TResult> source, DateTime afterDate)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");
        //    else if (afterDate.Kind != DateTimeKind.Utc)
        //        throw new ArgumentException("afterDate must be in UTC", "afterDate");

        //    return source.InvalidateIf(c => c.CachedDate > afterDate);
        //}

        /// <summary>
        /// Use the previous cached value if the retrieval mechanism failed
        /// </summary>
        public static CacheStrategy<T> IfRetrievalFailsUsePreviousValue<T>(this CacheStrategy<T> source)
        {
            return source.IfRetrievalFails(RetrievalErrorHandlerResult<T>.UsePreviousCachedValue);
        }

        /// <summary>
        /// Use the previous cached value (or a default value if there is no previous value) if the retrieval mechanism failed
        /// </summary>
        public static CacheStrategy<T> IfRetrievalFailsUsePreviousValueOrDefault<T>(this CacheStrategy<T> source,  T defaultValue)
        {
            return source.IfRetrievalFails((ex, cachedValue) => RetrievalErrorHandlerResult<T>.UsePreviousCachedValueOrDefault(ex, cachedValue, defaultValue));
        }

        /// <summary>
        /// Use the previous cached value if the retrieval mechanism failed
        /// </summary>
        public static CacheStrategyAsync<T> IfRetrievalFailsUsePreviousValue<T>(this CacheStrategyAsync<T> source)
        {
            return source.IfRetrievalFails(RetrievalErrorHandlerResult<T>.UsePreviousCachedValue);
        }

        /// <summary>
        /// Use the previous cached value (or a default value if there is no previous value) if the retrieval mechanism failed
        /// </summary>
        public static CacheStrategyAsync<T> IfRetrievalFailsUsePreviousValueOrDefault<T>(this CacheStrategyAsync<T> source, T defaultValue)
        {
            return source.IfRetrievalFails((ex, cachedValue) => RetrievalErrorHandlerResult<T>.UsePreviousCachedValueOrDefault(ex, cachedValue, defaultValue));
        }


        /// <summary>
        /// Returns a caching strategy for the specified method. The cache source type and method name will be used for the cache key and the method will be used as the retrieval mechanism
        /// </summary>
        public static CacheStrategy<TResult> Method<T, TResult>(this Cache<T> cache, Expression<Func<T, TResult>> method)
        {
            ExpressionAnalyzer analyzer = new ExpressionAnalyzer();
            return analyzer.CreateCacheStrategy(cache, method)
                           .RetrieveUsingMethod();
        }

        /// <summary>
        /// Returns an async caching strategy for the specified method. The cache source type and method name will be used for the cache key and the method will be used as the retrieval mechanism if the item is not found in the cache
        /// </summary>
        public static CacheStrategyAsync<TResult> Method<T, TResult>(this Cache<T> cache, Expression<Func<T, Task<TResult>>> method)
        {
            ExpressionAnalyzer analyzer = new ExpressionAnalyzer();
            return analyzer.CreateAsyncCacheStrategy(cache, method)
                           .RetrieveUsingMethod();
        }

        /// <summary>
        /// Returns a caching strategy that cached individual items returned from the method by their specified key. The cache source type, method name, and item key will be used for the cache key and the method will be used as the retrieval mechanism for items not found in the cache
        /// </summary>
        public static BulkCacheStrategy<TKey, TResult> Method<T, TKey, TResult>(this Cache<T> cache, Expression<Func<T, ICollection<KeyValuePair<TKey, TResult>>>> method)
        {
            ExpressionAnalyzer analyzer = new ExpressionAnalyzer();
            return analyzer.CreateBulkCacheStrategy(cache, method)
                           .RetrieveUsingMethod();
        }

        /// <summary>
        /// Returns an async caching strategy that cached individual items returned from the method by their specified key. The cache source type, method name, and item key will be used for the cache key and the method will be used as the retrieval mechanism for items not found in the cache
        /// </summary>
        public static BulkCacheStrategyAsync<TKey, TResult> Method<T, TKey, TResult>(this Cache<T> cache, Expression<Func<T, Task<ICollection<KeyValuePair<TKey, TResult>>>>> method)
        {
            ExpressionAnalyzer analyzer = new ExpressionAnalyzer();
            return analyzer.CreateAsyncBulkCacheStrategy(cache, method)
                           .RetrieveUsingMethod();
        } 
    
    }
}
