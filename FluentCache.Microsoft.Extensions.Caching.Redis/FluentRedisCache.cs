using FluentCache.Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCache.Microsoft.Extensions.Caching.Redis
{
    /// <summary>
    /// A FluentCache.ICache wrapper using Redis
    /// </summary>
    public class FluentRedisCache : FluentIDistributedCache
    {
        private static IDistributedCache CreateCache(string instance, string configuration)
        {
            var options = new RedisCacheOptions
            {
                Configuration = configuration,
                InstanceName = instance
            };
            return new RedisCache(options);
        }

        private static ISerializer CreateSerializer()
        {
            return new JsonSerializer();
        }

        /// <summary>
        /// Constructs a FluentRedisCache for the specified instance
        /// </summary>
        public FluentRedisCache(string instance, string configuration)
            : this(instance, configuration, CreateSerializer())
        {

        }

        /// <summary>
        /// Constructs a FluentRedisCache for the specified instance using the specified serializer
        /// </summary>
        public FluentRedisCache(string instance, string configuration, ISerializer serializer)
            : base(CreateCache(instance, configuration), serializer)
        {

        }
    }
}
