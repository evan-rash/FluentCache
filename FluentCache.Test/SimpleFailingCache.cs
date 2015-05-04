using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    public class SimpleFailingCache : ICache
    {
        public SimpleFailingCache(bool handleErrors, IEnumerable<CacheOperation> failingOperations)
        {
            HandleErrors = handleErrors;
            Values = new Dictionary<string, SimpleCachedValue>();
            ParameterCacheKeyProvider = new DefaultParameterCacheKeyProvider();
            Fail = new HashSet<CacheOperation>(failingOperations);
        }

        private readonly HashSet<CacheOperation> Fail;
        private readonly bool HandleErrors;


        private Dictionary<string, SimpleCachedValue> Values;
        private string GetItemKey(string key, string region)
        {
            return String.Format("{0}-{1}", key, region);
        }
        private SimpleCachedValue GetItemCore(string key, string region)
        {
            if (Fail.Contains(CacheOperation.Get))
                throw new FluentCacheException("Not implemented!");

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
            if (Fail.Contains(CacheOperation.Set))
                throw new FluentCacheException("Not implemented!");



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
            if (Fail.Contains(CacheOperation.Remove))
                throw new FluentCacheException("Not implemented!");

            Values.Remove(GetItemKey(key, region));
        }

        public IParameterCacheKeyProvider ParameterCacheKeyProvider { get; private set; }

        bool ICache.TryHandleCachingFailure(FluentCacheException exception, CacheOperation cacheOperation)
        {
            return HandleErrors;
        }
    }
}
