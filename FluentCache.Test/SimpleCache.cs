using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    public class SimpleCache : ICache
    {
        public SimpleCache(IParameterCacheKeyProvider parameterCacheKeyProvider)
        {
            Values = new Dictionary<string, SimpleCachedValue>();
            ParameterCacheKeyProvider = parameterCacheKeyProvider;

        }
        public SimpleCache()
            : this(new DefaultParameterCacheKeyProvider())
        {
        }

        private Dictionary<string, SimpleCachedValue> Values;
        private string GetItemKey(string key, string region)
        {
            return String.Format("{0}-{1}", key, region);
        }
        private SimpleCachedValue GetItemCore(string key, string region)
        {
            string itemKey = GetItemKey(key, region);
            
            SimpleCachedValue val;
            if (!Values.TryGetValue(itemKey, out val))
                return null;

            else if (val.CheckAccess(DateTime.UtcNow))
            {
                Values.Remove(itemKey);
                return null;
            }

            return val;
        }
        private SimpleCachedValue SetItemCore(string key, string region, object value, CachePolicy cachePolicy)
        {
            DateTime now = DateTime.UtcNow;
            string itemKey = GetItemKey(key, region);
            var cacheValue = new SimpleCachedValue
            {
                CachedDate = now,
                ExpireAbsolute = cachePolicy.ExpirationDate,
                ExpireAfter = cachePolicy.SlidingExpiration,
                Value = value
            };

            Values[itemKey] = cacheValue;

            return cacheValue;
        }
        public ICachedValue Get(string key, string region)
        {
            return GetItemCore(key, region);
        }
        public ICachedValue<T> Get<T>(string key, string region)
        {
            SimpleCachedValue val = GetItemCore(key, region);
            return val == null ? null : val.Wrap<T>();
        }
        public ICachedValue Set(string key, string region, object value, CachePolicy cachePolicy)
        {
            return SetItemCore(key, region, value, cachePolicy);
        }
        public ICachedValue<T> Set<T>(string key, string region, T value, CachePolicy cachePolicy)
        {
            return SetItemCore(key, region, value, cachePolicy).Wrap<T>();
        }
        public void Remove(string key, string region)
        {
            Values.Remove(GetItemKey(key, region));
        }


        public IParameterCacheKeyProvider ParameterCacheKeyProvider { get; private set; }

        bool ICache.TryHandleCachingFailure(FluentCacheException exception, CacheOperation cacheOperation)
        {
            return false;
        }
    }
}
