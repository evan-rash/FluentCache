using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Defines a cached value
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    public class CachedValue<T>
    {
        /// <summary>
        /// Gets the date the value was first cached
        /// </summary>
        public DateTime CachedDate { get; set; }

        /// <summary>
        /// Gets the date the cached value was last validated, or null if it has not been validated
        /// </summary>
        public DateTime LastValidatedDate { get; set; }

        /// <summary>
        /// Gets the version of this cached value
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Gets the cached value
        /// </summary>
        public T Value { get; set; }
    }

}