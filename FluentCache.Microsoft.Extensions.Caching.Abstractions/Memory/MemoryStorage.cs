using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCache.Microsoft.Extensions.Caching.Memory
{
    internal class MemoryStorage
    {
        public DateTime CacheDate { get; set; }
        public DateTime LastValidatedDate { get; set; }
        public long Version { get; set; }
        public object Value { get; set; }

        public CachedValue<T> ToCachedValue<T>()
        {
            if (!(Value is T))
                return null;

            return new CachedValue<T>
            {
                CachedDate = CacheDate,
                LastValidatedDate = LastValidatedDate,
                Value = (T)Value,
                Version = Version
            };
        }

    }
}
