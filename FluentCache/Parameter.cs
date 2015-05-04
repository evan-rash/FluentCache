using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Methods to configure how the fluent caching API handles parameters
    /// </summary>
    public static class Parameter
    {
        /// <summary>
        /// Informs the fluent caching API that the specified parameter should not be included when determining the cache key
        /// </summary>
        public static T DoNotCache<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Informs the fluent caching API that the specified parameter should not be included when determining the cache key. Use the value parameter if a value is needed for data retrieval
        /// </summary>
        public static T DoNotCache<T>(T value)
        {
            return value;
        }
    }

}
