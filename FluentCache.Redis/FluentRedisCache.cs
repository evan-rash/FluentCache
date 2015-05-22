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
    public class FluentRedisCache : Cache
    {
        /// <summary>
        /// Constructs a new cache instance for the desired redis connection
        /// </summary>
        public FluentRedisCache(ConnectionMultiplexer redis)
        {
            Redis = redis;
        }

        private readonly ConnectionMultiplexer Redis;

        private class Hashes
        {
            public const string CachedDateTicks = "CachedDateTicks";
            public const string LastValidatedDateTicks = "LastValidatedDateTicks";
            public const string Value = "Value";
            public const string LastAccessedDateTicks = "LastAccessedDateTicks";
            public const string Version = "Version";
            public const string SlidingExpirationTicks = "SlidingExpirationTicks";
        }
        private class Storage
        {
            public string Key { get; set; }
            public DateTime CachedDate { get; set; }
            public DateTime LastValidatedDate { get; set; }
            public string SerializedValue { get; set; }
            public long Version { get; set; }

            public CachedValue<T> ToCachedValue<T>(FluentRedisCache cache)
            {
                return new CachedValue<T>
                {
                    CachedDate = CachedDate,
                    LastValidatedDate = LastValidatedDate,
                    Value = cache.DeserializeObjectFromRedisCache<T>(SerializedValue),
                    Version = Version
                };
            }
        }
        
        private RedisKey MakeKey(string key, string region)
        {
            return region + ":" + key;
        }
        private Storage GetRedis(string key, string region)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                IDatabase database = Redis.GetDatabase();
                RedisKey redisKey = MakeKey(key, region);

                ITransaction getTransaction = database.CreateTransaction();
                getTransaction.AddCondition(Condition.KeyExists(redisKey));
                Task<HashEntry[]> hashTask = getTransaction.HashGetAllAsync(redisKey);

                //if the transaction didn't execute it's because the hash doesn't exist or the :sliding-expired key has expired
                if (!getTransaction.Execute(CommandFlags.None))
                    return null;

                HashEntry[] values = hashTask.Result;

                HashEntry version = values.FirstOrDefault(v => v.Name == Hashes.Version);
                HashEntry cachedDateTicks = values.FirstOrDefault(v => v.Name == Hashes.CachedDateTicks);
                HashEntry lastValidatedTicks = values.FirstOrDefault(v => v.Name == Hashes.LastValidatedDateTicks);
                HashEntry serializedValue = values.FirstOrDefault(v => v.Name == Hashes.Value);
                HashEntry lastAccessedTicks = values.FirstOrDefault(v => v.Name == Hashes.LastAccessedDateTicks);
                HashEntry slidingExpirationTicks = values.FirstOrDefault(v => v.Name == Hashes.SlidingExpirationTicks);

                if (cachedDateTicks == null || !cachedDateTicks.Value.HasValue)
                    return null;

                DateTime cacheDate = new DateTime((long)cachedDateTicks.Value);
                DateTime? lastValidatedDate = lastValidatedTicks == null
                                            || !lastValidatedTicks.Value.HasValue
                                            || (long)lastValidatedTicks.Value == -1L ? default(DateTime?) : new DateTime((long)lastValidatedTicks.Value);
                DateTime? lastAccessedDate = lastAccessedTicks == null
                                            || !lastAccessedTicks.Value.HasValue
                                            || (long)lastValidatedTicks.Value == -1L ? default(DateTime?) : new DateTime((long)lastAccessedTicks.Value);
                TimeSpan? slidingExpiration = slidingExpirationTicks == null
                                            || !slidingExpirationTicks.Value.HasValue
                                            || (long)slidingExpirationTicks.Value == -1L ? default(TimeSpan?) : TimeSpan.FromTicks((long)slidingExpirationTicks.Value);

                string value = serializedValue == null ? null : (string)serializedValue.Value;

                //Note: need to take one off of version to conform to initial version = 0 standard
                long ver = version == null ? 0L : (((long?)version.Value) - 1).GetValueOrDefault();

                //mark the record as updated and re-set the sliding expiration date
                ITransaction markUpdatedTransaction = database.CreateTransaction();
                markUpdatedTransaction.HashSetAsync(redisKey, Hashes.LastAccessedDateTicks, now.Ticks);
                if (slidingExpirationTicks != null)
                    markUpdatedTransaction.KeyExpireAsync(redisKey, now + slidingExpiration);
                
                markUpdatedTransaction.Execute();

                return new Storage
                {
                    CachedDate = cacheDate,
                    LastValidatedDate = lastValidatedDate.GetValueOrDefault(cacheDate),
                    SerializedValue = value,
                    Version = ver
                };
            }
            catch (StackExchange.Redis.RedisException x)
            {
                throw ExceptionFactory.CachingFailed(CacheOperation.Get, x);
            }
        }
        private Storage SetRedis(string key, string region, object value, CacheExpiration expiration)
        {
            try
            {
                RedisKey redisKey = MakeKey(key, region);
                string serializedValue = SerializeObjectToCacheInRedis(value);
                DateTime now = DateTime.UtcNow;
                var hashEntries = new[] { 
                        new HashEntry(Hashes.CachedDateTicks, now.Ticks),
                        new HashEntry(Hashes.LastValidatedDateTicks, -1L), 
                        new HashEntry(Hashes.SlidingExpirationTicks, expiration.SlidingExpiration == null? -1L : expiration.SlidingExpiration.Value.Ticks),
                        new HashEntry(Hashes.Value, serializedValue) };

                IDatabase database = Redis.GetDatabase();
                ITransaction setTransaction = database.CreateTransaction();
                setTransaction.HashSetAsync(redisKey, hashEntries);
                setTransaction.KeyExpireAsync(redisKey, now + expiration.SlidingExpiration);
                Task<long> versionTask = setTransaction.HashIncrementAsync(redisKey, Hashes.Version);
               
                setTransaction.Execute();

                long version = versionTask.Result - 1L;

                return new Storage { CachedDate = now, LastValidatedDate = now, SerializedValue = serializedValue, Version = version };
            }
            catch (StackExchange.Redis.RedisException x)
            {
                throw ExceptionFactory.CachingFailed(CacheOperation.Set, x);
            }
        }
        private void MarkValidatedRedis(string key, string region, DateTime validatedDate)
        {
            
            try
            {
                RedisKey redisKey = MakeKey(key, region);
                Redis.GetDatabase().HashSet(redisKey, Hashes.LastValidatedDateTicks, validatedDate.Ticks);
            }
            catch (StackExchange.Redis.RedisException x)
            {
                throw ExceptionFactory.CachingFailed(CacheOperation.MarkValidated, x);
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
                throw ExceptionFactory.CachingFailed(CacheOperation.Remove, x);
            }
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

        /// <summary>
        /// Gets the value in the cache
        /// </summary>
        public override CachedValue<T> Get<T>(string key, string region)
        {
            Storage storage = GetRedis(key, region);
            return storage == null ? null : storage.ToCachedValue<T>(this);
        }

        /// <summary>
        /// Sets the value in the cache
        /// </summary>
        public override CachedValue<T> Set<T>(string key, string region, T value, CacheExpiration cacheExpiration)
        {
            Storage storage = SetRedis(key, region, value, cacheExpiration);
            return storage.ToCachedValue<T>(this);
        }

        /// <summary>
        /// Marks the specified cached item as validated
        /// </summary>
        protected override void MarkAsValidated(string key, string region)
        {
            MarkValidatedRedis(key, region, DateTime.UtcNow);
        }

        /// <summary>
        /// Removes the specified cached item from the cache
        /// </summary>
        public override void Remove(string key, string region)
        {
            RemoveRedis(key, region);
        }
    }
}
