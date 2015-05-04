using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A strategy to access or update data in a cache
    /// </summary>
    public class CacheStrategy
    {
        internal CacheStrategy(ICache cache, string baseKey)
        {
            _Cache = cache;
            _BaseKey = baseKey;
            Parameters = new List<object>();
        }

        private readonly ICache _Cache;
        private readonly string _BaseKey;

        internal ICache Cache { get { return _Cache; } }
        internal string BaseKey { get { return _BaseKey; } }

        internal IReadOnlyList<object> Parameters { get; set; }

        internal P GetParameter<P>(int index)
        {
            return (P)Parameters[index];
        }

        internal virtual void CopyFrom(CacheStrategy other)
        {
            this.CachePolicy = other.CachePolicy;
            this.Region = other.Region;
            this.Parameters = other.Parameters == null ? null : other.Parameters.ToList();
        }

        /// <summary>
        /// Gets the Region that this caching policy will use when caching items
        /// </summary>
        public string Region { get; internal set; }
        
        internal CachePolicy CachePolicy { get; set; }

        /// <summary>
        /// Gets the Key that this caching policy will using when caching items
        /// </summary>
        public string Key
        {
            get
            {
                if (Parameters == null || !Parameters.Any())
                {
                    return String.Format("{0}", BaseKey);
                }
                else
                {
                    IParameterCacheKeyProvider paramCacheKeyProvider = Cache.ParameterCacheKeyProvider;
                    if (paramCacheKeyProvider == null)
                        throw new InvalidOperationException("Cache must have a ParameterCacheKeyProvider specified in order to generate unique keys for parameterized caching strategies");

                    return String.Format("{0}.{1}", BaseKey, String.Join(".", Parameters.Select(p => paramCacheKeyProvider.GenerateParameterCacheKey(p))));
                }
            }
        }


    }


}
