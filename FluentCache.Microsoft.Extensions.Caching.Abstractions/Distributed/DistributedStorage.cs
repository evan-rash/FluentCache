using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCache.Microsoft.Extensions.Caching.Distributed
{
    internal class DistributedStorage
    {
        public DateTime CacheDate { get; set; }
        public DateTime LastValidatedDate { get; set; }
        public long Version { get; set; }
        public string Value { get; set; }

        public CachedValue<T> ToCachedValue<T>(ISerializer serializer)
        {
            return new CachedValue<T>
            {
                CachedDate = CacheDate,
                LastValidatedDate = LastValidatedDate,
                Value = serializer.Deserialize<T>(Value),
                Version = Version
            };
        }

        public byte[] ToBytes(ISerializer serializer)
        {
            string serializedValue = serializer.Serialize(this);
            return Encoding.UTF8.GetBytes(serializedValue);
        }

        public static DistributedStorage FromBytes(ISerializer serializer, byte[] bytes)
        {
            string serializedValue = Encoding.UTF8.GetString(bytes);
            return serializer.Deserialize<DistributedStorage>(serializedValue);
        }

    }
}
