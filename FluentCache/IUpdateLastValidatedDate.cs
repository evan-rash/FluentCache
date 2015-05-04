using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Allows a caching implementation to update the last validated date of a cached value. This is an internal interface not intended for direct public consumption
    /// </summary>
    public interface IUpdateLastValidatedDate
    {
        /// <summary>
        /// Mark the specified cached value as updated
        /// </summary>
        void MarkAsValidated(ICachedValue cachedValue, DateTime validatedDate);
    }
}
