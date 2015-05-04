using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A strategy to asynchronously access or modify strongly-typed data in a Cache
    /// </summary>
    public class BulkCacheStrategy<TKey, TResult> : BulkCacheStrategyIncomplete<TKey, TResult>
    {
        internal BulkCacheStrategy(ICache cache, string baseKey, ICollection<TKey> keys)
            : base(cache, baseKey, keys)
        {
        }

        internal Func<ICachedValue<TResult>, CacheValidationResult> ValidateCallback { get; set; }
        internal Func<ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>> RetrieveCallback { get; set; }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns CacheValidationResult.Invalid
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public BulkCacheStrategy<TKey, TResult> Validate(Func<ICachedValue<TResult>, CacheValidationResult> validate)
        {
            this.ValidateCallback = validate;
            return this;
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns false
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public BulkCacheStrategy<TKey, TResult> InvalidateIf(Func<ICachedValue<TResult>, bool> validate)
        {
            Func<ICachedValue<TResult>, CacheValidationResult> val = existing => validate(existing) ? CacheValidationResult.Valid : CacheValidationResult.Invalid;
            this.ValidateCallback = val;
            return this;
        }

        /// <summary>
        /// Gets all cached items
        /// </summary>
        public IList<ICachedValue<TResult>> GetAll()
        {
            var keysToLoad = Keys.ToList();
            var results = new List<ICachedValue<TResult>>(Keys.Count);

            foreach (TKey key in Keys)
            {
                string itemKey = GetItemKey(key);
                CacheStrategy<TResult> itemStrategy = new CacheStrategy<TResult>(Cache, itemKey).WithRegion(Region);

                if (ValidateCallback != null)
                    itemStrategy = itemStrategy.Validate(ValidateCallback);

                ICachedValue<TResult> cachedValue = itemStrategy.Get();
                if (cachedValue != null)
                {
                    keysToLoad.Remove(key);
                    results.Add(cachedValue);
                }
            }

            if (RetrieveCallback != null)
            {
                ICollection<KeyValuePair<TKey, TResult>> newResults = RetrieveCallback(keysToLoad);

                foreach (KeyValuePair<TKey, TResult> result in newResults)
                {
                    string itemKey = GetItemKey(result.Key);
                    TResult value = result.Value;

                    ICachedValue<TResult> cachedValue = Cache.Set(itemKey, Region, value, CachePolicy);

                    results.Add(cachedValue);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets all cached values
        /// </summary>
        public IList<TResult> GetAllValues()
        {
            IList<ICachedValue<TResult>> results = GetAll();
            return results.Select(s => s.Value)
                          .ToList();
        }
    }

}
