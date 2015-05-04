using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace FluentCache.Redis
{
    /// <summary>
    /// A FluentCache implementation that uses Redis to store and retrieve cache values
    /// </summary>
    public class FluentRedisCache : ICache, IUpdateLastValidatedDate
    {
        /// <summary>
        /// Constructs a new cache instance for the desired redis connection. Allows for customizing the parameter caching strategy and the cache value serialization
        /// </summary>
        public FluentRedisCache(ConnectionMultiplexer redis, IParameterCacheKeyProvider parameterCacheKeyProvider)
        {
            ParameterCacheKeyProvider = parameterCacheKeyProvider;
            Redis = redis;
        }

        /// <summary>
        /// Constructs a new cache instance for the desired redis connection. Uses the default parameter caching strategy and cache value serialization
        /// </summary>
        public FluentRedisCache(ConnectionMultiplexer redis)
            : this(redis, new DefaultParameterCacheKeyProvider())
        {
        }

        private readonly IParameterCacheKeyProvider ParameterCacheKeyProvider;
        private readonly ConnectionMultiplexer Redis;

        private class Keys
        {
            public static RedisKey SlidingExpiration(RedisKey rootKey)
            {
                return rootKey + ":sliding-expiration";
            }
            public static RedisKey Version(RedisKey rootKey)
            {
                return rootKey + ":version";
            }
        }
        private class Hashes
        {
            public const string CachedDateTicks = "CachedDateTicks";
            public const string LastValidatedDateTicks = "LastValidatedDateTicks";
            public const string Value = "Value";
            public const string LastAccessedDateTicks = "LastAccessedDateTicks";
        }
        private class RedisCachedValue : ICachedValueWithVersion 
        {
            public RedisCachedValue(string key, FluentRedisCache cache)
            {
                Key = key;
                Cache = cache;
            }

            private readonly FluentRedisCache Cache;

            public string Key { get; set; }
            public DateTime CachedDate { get; set; }
            public DateTime? LastValidatedDate { get; set; }
            public string SerializedValue { get; set; }
            object ICachedValue.Value { get { return SerializedValue; } }

            public long? Version { get; set; }

            public RedisCachedValue<T> Wrap<T>()
            {
                return new RedisCachedValue<T>(Key, Cache)
                {
                    Value = Cache.DeserializeObjectFromRedisCache<T>(SerializedValue),
                    CachedDate = this.CachedDate,
                    LastValidatedDate = this.LastValidatedDate,
                    Version = this.Version,
                    Key = this.Key,
                };
            }

            public void MarkAsValidated(DateTime validatedDate)
            {
                Cache.MarkValidatedRedis(Key, validatedDate);
            }
        }
        private class RedisCachedValue<T> : ICachedValueWithVersion<T>
        {
            public RedisCachedValue(string key, FluentRedisCache cache)
            {
                Key = key;
                Cache = cache;
            }

            private readonly FluentRedisCache Cache;
            public string Key { get; set; }
            public T Value { get; set; }

            public DateTime CachedDate { get; set; }

            public DateTime? LastValidatedDate { get; set; }

            public long? Version { get; set; }

            object ICachedValue.Value { get { return Value; } }

            public void MarkAsValidated(DateTime validatedDate)
            {
                Cache.MarkValidatedRedis(Key, validatedDate);
            }
        }

        private RedisKey MakeKey(string key, string region)
        {
            return region + ":" + key;
        }
        private RedisCachedValue GetRedis(string key, string region)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                IDatabase database = Redis.GetDatabase();
                RedisKey redisKey = MakeKey(key, region);

                ITransaction getTransaction = database.CreateTransaction();
                getTransaction.AddCondition(Condition.KeyExists(redisKey));
                getTransaction.AddCondition(Condition.KeyExists(Keys.SlidingExpiration(redisKey)));
                Task<HashEntry[]> hashTask = getTransaction.HashGetAllAsync(redisKey);
                Task<RedisValue> versionTask = getTransaction.StringGetAsync(Keys.Version(redisKey));
                Task<RedisValue> slidingExpirationTask = getTransaction.StringGetAsync(Keys.SlidingExpiration(redisKey));

                //if the transaction didn't execute it's because the hash doesn't exist or the :sliding-expired key has expired
                if (!getTransaction.Execute(CommandFlags.None))
                    return null;

                HashEntry[] values = hashTask.Result;
                RedisValue version = versionTask.Result;
                RedisValue slidingExpirationTicks = slidingExpirationTask.Result;
                HashEntry cachedDateTicks = values.FirstOrDefault(v => v.Name == Hashes.CachedDateTicks);
                HashEntry lastValidatedTicks = values.FirstOrDefault(v => v.Name == Hashes.LastValidatedDateTicks);
                HashEntry serializedValue = values.FirstOrDefault(v => v.Name == Hashes.Value);
                HashEntry lastAccessedTicks = values.FirstOrDefault(v => v.Name == Hashes.LastAccessedDateTicks);

                if (cachedDateTicks == null || !cachedDateTicks.Value.HasValue)
                    return null;

                DateTime cacheDate = new DateTime((long)cachedDateTicks.Value);
                DateTime? lastValidatedDate = lastValidatedTicks == null
                                            || !lastValidatedTicks.Value.HasValue
                                            || (long)lastValidatedTicks.Value == -1L ? default(DateTime?) : new DateTime((long)lastValidatedTicks.Value);
                DateTime? lastAccessedDate = lastAccessedTicks == null
                                            || !lastAccessedTicks.Value.HasValue
                                            || (long)lastValidatedTicks.Value == -1L ? default(DateTime?) : new DateTime((long)lastAccessedTicks.Value);
                TimeSpan? slidingExpiration = !slidingExpirationTicks.HasValue
                                            || slidingExpirationTicks == -1L ? default(TimeSpan?) : TimeSpan.FromTicks((long)slidingExpirationTicks);

                string value = serializedValue == null ? null : (string)serializedValue.Value;

                //mark the record as updated and re-set the sliding expiration date
                ITransaction markUpdatedTransaction = database.CreateTransaction();
                markUpdatedTransaction.HashSetAsync(redisKey, Hashes.LastAccessedDateTicks, now.Ticks);
                markUpdatedTransaction.KeyExpireAsync(Keys.SlidingExpiration(redisKey), now + slidingExpiration);
                markUpdatedTransaction.Execute();

                return new RedisCachedValue(redisKey, this)
                {
                    CachedDate = cacheDate,
                    LastValidatedDate = lastValidatedDate,
                    SerializedValue = value,
                    Version = (long?)version
                };
            }
            catch (StackExchange.Redis.RedisException x)
            {
                throw ExceptionFactory.CachingFailed(x);
            }
        }
        private RedisCachedValue SetRedis(string key, string region, object value, CachePolicy cachePolicy)
        {
            try
            {
                RedisKey redisKey = MakeKey(key, region);
                string serializedValue = SerializeObjectToCacheInRedis(value);
                DateTime now = DateTime.UtcNow;
                var hashEntries = new[] { new HashEntry(Hashes.CachedDateTicks, now.Ticks), new HashEntry(Hashes.LastValidatedDateTicks, -1), new HashEntry(Hashes.Value, serializedValue) };

                IDatabase database = Redis.GetDatabase();
                ITransaction setTransaction = database.CreateTransaction();
                setTransaction.HashSetAsync(redisKey, hashEntries);
                setTransaction.KeyExpireAsync(redisKey, cachePolicy.ExpirationDate);
                setTransaction.StringSetAsync(Keys.SlidingExpiration(redisKey), cachePolicy.SlidingExpiration == null ? -1L : cachePolicy.SlidingExpiration.Value.Ticks);
                setTransaction.KeyExpireAsync(Keys.SlidingExpiration(redisKey), now + cachePolicy.SlidingExpiration);
                Task<long> versionTask = setTransaction.StringIncrementAsync(Keys.Version(redisKey));

                setTransaction.Execute();

                long version = versionTask.Result;

                return new RedisCachedValue(redisKey, this) { CachedDate = now, LastValidatedDate = null, SerializedValue = serializedValue, Version = version };
            }
            catch (StackExchange.Redis.RedisException x)
            {
                throw ExceptionFactory.CachingFailed(x);
            }
        }
        private void MarkValidatedRedis(string key, DateTime validatedDate)
        {
            try
            {
                Redis.GetDatabase().HashSet(key, "LastValidatedDateTicks", validatedDate.Ticks);
            }
            catch (StackExchange.Redis.RedisException x)
            {
                throw ExceptionFactory.CachingFailed(x);
            }
        }
        private void RemoveRedis(string key, string region)
        {
            try
            {
                RedisKey redisKey = MakeKey(key, region);
                Redis.GetDatabase().KeyDelete(redisKey);
            }
            catch(StackExchange.Redis.RedisException x)
            {
                throw ExceptionFactory.CachingFailed(x);
            }
        }

        /// <summary>
        /// Gets the desired value from the cache
        /// </summary>
        public ICachedValue Get(string key, string region)
        {
            return GetRedis(key, region);
        }

        /// <summary>
        /// Gets the desired value from the cache
        /// </summary>
        public ICachedValue<T> Get<T>(string key, string region)
        {
            var val = Get(key, region) as RedisCachedValue;
            if (val == null)
                return null;

            return val.Wrap<T>();
        }

        /// <summary>
        /// Sets the value in the cache
        /// </summary>
        public ICachedValue Set(string key, string region, object value, CachePolicy cachePolicy)
        {
            return SetRedis(key, region, value, cachePolicy);
        }

        /// <summary>
        /// Sets the value in the cache
        /// </summary>
        public ICachedValue<T> Set<T>(string key, string region, T value, CachePolicy cachePolicy)
        {
            var cachedValue = Set(key, region, (object)value, cachePolicy) as RedisCachedValue;
            return cachedValue.Wrap<T>();
        }

        /// <summary>
        /// Removes the value from the cache
        /// </summary>
        public void Remove(string key, string region)
        {
            RemoveRedis(key, region);
        }

        IParameterCacheKeyProvider ICache.ParameterCacheKeyProvider { get { return ParameterCacheKeyProvider; } }

        void IUpdateLastValidatedDate.MarkAsValidated(ICachedValue cachedValue, DateTime validatedDate)
        {
            var redisCachedValue = cachedValue as RedisCachedValue;
            if (redisCachedValue == null)
                return;

            MarkValidatedRedis(redisCachedValue.Key, validatedDate);
            redisCachedValue.LastValidatedDate = validatedDate;
        }

        /// <summary>
        /// Attempts to handle caching failures. Return true to indicate that the error is handled and execution should continue
        /// </summary>
        protected virtual bool TryHandleCachingFailure(FluentCacheException exception, CacheOperation cacheOperation)
        {
            return exception.InnerException as StackExchange.Redis.RedisException != null;
        }

        /// <summary>
        /// Serializes the specified cache value into a string to be stored in redis
        /// </summary>
        protected virtual string SerializeObjectToCacheInRedis(object objectToSerialize)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(objectToSerialize);
        }

        /// <summary>
        /// Deserializes the redis cached value into the specified type
        /// </summary>
        protected virtual T DeserializeObjectFromRedisCache<T>(string serializedObject)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serializedObject);
        }


        bool ICache.TryHandleCachingFailure(FluentCacheException exception, CacheOperation cacheOperation)
        {
            return TryHandleCachingFailure(exception, cacheOperation);
        }
    }
}
