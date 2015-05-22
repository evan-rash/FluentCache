using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Defines a list of caching operations
    /// </summary>
    public enum CacheOperation
    {
        /// <summary>
        /// Getting a value from the cache
        /// </summary>
        Get = 0,
        /// <summary>
        /// Setting a value in the cache
        /// </summary>
        Set = 1,
        /// <summary>
        /// Removing a value from the cache
        /// </summary>
        Remove = 2,
        /// <summary>
        /// Marking a value in the cache as validated
        /// </summary>
        MarkValidated = 3,
    }
}
