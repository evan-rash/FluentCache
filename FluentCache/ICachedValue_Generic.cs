using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A strongly typed value that has been retrieved from a cache
    /// </summary>
    /// <typeparam name="T">The type of the vaue that is to be retrieved</typeparam>
    public interface ICachedValue<T> : ICachedValue
    {
        /// <summary>
        /// Gets the cached value
        /// </summary>
        new T Value { get; }
    }
}
