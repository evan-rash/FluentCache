using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    internal class CacheWithSource<TSource> : ICache<TSource>
    {
        public CacheWithSource(TSource source, ICache cache)
        {
            Source = source;
            Cache = cache;
        }
        private readonly ICache Cache;
        private readonly TSource Source;

        TSource ICache<TSource>.Source { get { return Source; } }

        ICachedValue ICache.Get(string key, string region)
        {
            return Cache.Get(key, region);
        }

        ICachedValue<T> ICache.Get<T>(string key, string region)
        {
            return Cache.Get<T>(key, region);
        }

        ICachedValue ICache.Set(string key, string region, object value, CachePolicy cachePolicy)
        {
            return Cache.Set(key, region, value, cachePolicy);
        }

        ICachedValue<T> ICache.Set<T>(string key, string region, T value, CachePolicy cachePolicy)
        {
            return Cache.Set(key, region, value, cachePolicy);
        }

        void ICache.Remove(string key, string region)
        {
            Cache.Remove(key, region);
        }

        IParameterCacheKeyProvider ICache.ParameterCacheKeyProvider
        {
            get { return Cache.ParameterCacheKeyProvider; }
        }

        bool ICache.TryHandleCachingFailure(FluentCacheException exception, CacheOperation cacheOperation)
        {
            return Cache.TryHandleCachingFailure(exception, cacheOperation);
        }
    }
}
