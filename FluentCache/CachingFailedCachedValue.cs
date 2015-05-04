using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Represents a cached value that was generated from a failed caching operation
    /// </summary>
    public static class CachingFailedCachedValue
    {
        /// <summary>
        /// Creates a new ICachedValue wrapper around the specified value
        /// </summary>
        public static ICachedValue<T> Create<T>(T value)
        {
            return new CachedValue<T>
            {
                Value = value,
                CachedDate = DateTime.UtcNow
            };
        }

        private class CachedValue<T> : ICachedValue<T>
        {
            public CachedValue()
            {
            }

            public T Value { get; set; }

            public DateTime CachedDate { get; set; }

            public DateTime? LastValidatedDate { get { return null; } }

            object ICachedValue.Value { get { return Value; } }

            public long? Version { get { return -1L; } }
        }

    }

 }
