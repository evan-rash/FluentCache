using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Redis
{
    internal static class ExceptionFactory
    {
        public static FluentCacheException CachingFailed(StackExchange.Redis.RedisException redisException)
        {
            return new FluentCacheException("Caching failed while communicating with Redis. See inner exception for details", redisException);
        }
    }
}
