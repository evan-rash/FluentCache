using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Strategies
{
    /// <summary>
    /// An incomplete strategy to access or modify a batch of strongly-typed data in a Cache
    /// </summary>
    public class BulkCacheStrategyIncomplete<TKey, TResult> : CacheStrategy
    {
        internal BulkCacheStrategyIncomplete(Cache cache, string baseKey, ICollection<TKey> keys)
            : base(cache, baseKey)
        {
            Keys = keys.ToList();
        }

        internal readonly ICollection<TKey> Keys;

        internal string GetItemKey(TKey key)
        {
            return String.Format("{0}&itemkey={1}", Key, key);
        }

        /// <summary>
        /// Updates the cache strategy to include parameters
        /// </summary>
        public BulkCacheStrategyIncomplete<TKey, TResult> WithParameters(params object[] parameters)
        {
            this.Parameters = (parameters == null || !parameters.Any()) ? null : parameters.ToList();
            return this;
        }

        /// <summary>
        /// Updates the cache strategy to use the specified method to retrieve missing or invalid items in the batch
        /// </summary>
        public BulkCacheStrategy<TKey, TResult> RetrieveUsing(Func<ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>> retrieve)
        {
            var strat = new BulkCacheStrategy<TKey, TResult>(Cache, BaseKey, Keys);
            strat.CopyFrom(this);

            strat.RetrieveCallback = retrieve;
            return strat;
        }

        /// <summary>
        /// Updates the cache strategy to use the specified async method to retrieve missing or invalid items in the batch
        /// </summary>
        public BulkCacheStrategyAsync<TKey, TResult> RetrieveUsingAsync(Func<ICollection<TKey>, Task<ICollection<KeyValuePair<TKey, TResult>>>> retrieve)
        {
            var strat = new BulkCacheStrategyAsync<TKey, TResult>(Cache, BaseKey, Keys);
            strat.CopyFrom(this);

            strat.RetrieveCallback = retrieve;
            return strat;
        }

        /// <summary>
        /// Clears all values in the batch
        /// </summary>
        public void ClearValues()
        {
            foreach (TKey key in Keys)
            {
                string itemKey = GetItemKey(key);
                Cache.Remove(itemKey, Region);
            }
        }

        /// <summary>
        /// Sets the specified value in the cache
        /// </summary>
        public void SetValue(TKey key, TResult value)
        {
            string itemKey = GetItemKey(key);
            Cache.Set(itemKey, Region, value, Expiration);
        }

        /// <summary>
        /// Sets the specified values in the cache
        /// </summary>
        public void SetValues(ICollection<KeyValuePair<TKey, TResult>> values)
        {
            foreach (var kvp in values)
                SetValue(kvp.Key, kvp.Value);
        }

    }

}
