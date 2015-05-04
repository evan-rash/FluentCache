using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Defines possible outcomes when determining whether or not a cached value is still valid
    /// </summary>
    public enum CacheValidationResult
    {
        /// <summary>
        /// The cached value was determined to still be valid
        /// </summary>
        Valid = 0,

        /// <summary>
        /// The cached value was determined to be invalid
        /// </summary>
        Invalid = 1,

        /// <summary>
        /// The cached value was not validated and it is unknown whether or not the value is valid or invalid
        /// </summary>
        Unknown = 2
    }
}
