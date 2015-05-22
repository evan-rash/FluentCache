using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Redis
{
    internal static class ExceptionFactory
    {
        public static FluentCacheException CachingFailed(CacheOperation operation, StackExchange.Redis.RedisException redisException)
        {
            return new FluentCacheException("Caching failed while communicating with Redis. See inner exception for details", operation, redisException);
        }
    }
}
