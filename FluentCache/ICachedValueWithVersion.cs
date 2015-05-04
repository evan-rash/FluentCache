using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A value that has been retrieved from a cache that also includes an auto-incrementing version
    /// </summary>
    public interface ICachedValueWithVersion : ICachedValue
    {
        /// <summary>
        /// Returns an auto-incrementing sequence value indicating which version of the cached value is returned
        /// </summary>
        long? Version { get; }
    }

    /// <summary>
    /// A strongly-typed value that has been retrieved from a cache that also includes an auto-incrementing version
    /// </summary>
    public interface ICachedValueWithVersion<T> : ICachedValue<T>, ICachedValueWithVersion
    {
    }
}
