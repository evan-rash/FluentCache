using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.RuntimeCaching
{
    /// <summary>
    /// Provides a FluentCache.ICache wrapper around the System.Runtime.Caching.MemoryCache
    /// </summary>
    public class FluentMemoryCache : ICache, IUpdateLastValidatedDate
    {
        /// <summary>
        /// Creates a new FluentMemoryCache wrapper around MemoryCache.Default
        /// </summary>
        /// <returns></returns>
        public static ICache Default()
        {
            return Default(new DefaultParameterCacheKeyProvider());
        }

        /// <summary>
        /// Creates a new FluentMemoryCache wrapper around MemoryCache.Default with the specified parameter cache key provider
        /// </summary>
        public static ICache Default(IParameterCacheKeyProvider parameterCacheKeyProvider)
        {
            return new FluentMemoryCache(MemoryCache.Default, parameterCacheKeyProvider);
        }

        /// <summary>
        /// Creates a new FluentMemoryCache wrapper around the specified MemoryCache
        /// </summary>
        public FluentMemoryCache(MemoryCache memoryCache)
            : this(memoryCache, new DefaultParameterCacheKeyProvider())
        {
        }
        
        /// <summary>
        /// Creates a new FluentMemoryCache wrapper around the specified MemoryCache and IParameterCacheKeyProvider
        /// </summary>
        public FluentMemoryCache(MemoryCache memoryCache, IParameterCacheKeyProvider parameterCacheKeyProvider)
        {
            if (memoryCache == null)
                throw new ArgumentNullException("memoryCache");

            MemoryCache = memoryCache;
            _parameterCacheKeyProvider = parameterCacheKeyProvider;
        }

        private readonly MemoryCache MemoryCache;
        private readonly IParameterCacheKeyProvider _parameterCacheKeyProvider;

        private static string GetItemKey(string key, string region)
        {
            return String.Format("[{0}].[{1}]", region, key);
        }
        private class MemoryCachedValue : ICachedValue
        {
            public DateTime CachedDate { get; set; }

            public DateTime? LastValidatedDate { get; set; }

            public object Value { get; set; }

            public ICachedValue<T> Wrap<T>()
            {
                return new MemoryCachedValue<T>(this);
            }
        }
        private class MemoryCachedValue<T> : ICachedValue<T>
        {
            public MemoryCachedValue(MemoryCachedValue v)
            {
                CachedValue = v;
            }

            private readonly MemoryCachedValue CachedValue;

            public T Value { get { return (T)CachedValue.Value; } }

            public DateTime CachedDate { get { return CachedValue.CachedDate; } }

            public DateTime? LastValidatedDate
            {
                get { return CachedValue.LastValidatedDate; }
            }

            object ICachedValue.Value { get { return Value;  } }
        }

        private MemoryCachedValue GetItemCore(string key, string region)
        {
            string itemKey = GetItemKey(key, region);
            return MemoryCache.Get(itemKey) as MemoryCachedValue;           
        }
        private MemoryCachedValue SetItemCore(string key, string region, object value, CachePolicy policy)
        {
            string itemKey = GetItemKey(key, region);
            var cachePolicy = new CacheItemPolicy();

            if (policy.ExpirationDate != null)
                cachePolicy.AbsoluteExpiration = policy.ExpirationDate.GetValueOrDefault();
            if (policy.SlidingExpiration != null)
                cachePolicy.SlidingExpiration = policy.SlidingExpiration.GetValueOrDefault();

            var now = DateTime.UtcNow;

            var cacheItem = new MemoryCachedValue 
            {
                CachedDate = now,
                LastValidatedDate = now,
                Value = value
            };

            MemoryCache.Add(itemKey, cacheItem, cachePolicy);

            return cacheItem;
        }

        /// <summary>
        /// Gets the cached value for the specified key and region
        /// </summary>
        public ICachedValue Get(string key, string region)
        {
            return GetItemCore(key, region);
        }

        /// <summary>
        /// Gets the strongly typed cached value for the specified key and region
        /// </summary>
        public ICachedValue<T> Get<T>(string key, string region)
        {
            MemoryCachedValue val = GetItemCore(key, region);
            return val == null ? null : val.Wrap<T>();
        }

        /// <summary>
        /// Sets the cached value for the specified key and region
        /// </summary>
        public ICachedValue Set(string key, string region, object value, CachePolicy cachePolicy)
        {
            return SetItemCore(key, region, value, cachePolicy);
        }

        /// <summary>
        /// Sets the cached value for the specified key and region
        /// </summary>
        public ICachedValue<T> Set<T>(string key, string region, T value, CachePolicy cachePolicy)
        {
            return SetItemCore(key, region, value, cachePolicy).Wrap<T>();
        }

        /// <summary>
        /// Removes the cached value for the specified key and region
        /// </summary>
        public void Remove(string key, string region)
        {
            string itemKey = GetItemKey(key, region);
            MemoryCache.Remove(itemKey);
        }

        IParameterCacheKeyProvider ICache.ParameterCacheKeyProvider { get { return _parameterCacheKeyProvider; } }

        void IUpdateLastValidatedDate.MarkAsValidated(ICachedValue cachedValue, DateTime validatedDate)
        {
            var memoryCachedValue = cachedValue as MemoryCachedValue;
            memoryCachedValue.LastValidatedDate = validatedDate;
        }

        bool ICache.TryHandleCachingFailure(FluentCacheException exception, CacheOperation cacheOperation)
        {
            //If there are exceptions interacting with the memory cache we can't reasonably recover from them
            return false;
        }
    }
}
