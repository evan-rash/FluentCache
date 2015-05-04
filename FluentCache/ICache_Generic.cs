using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A strongly-typed caching source. This is an internal interface designed for use with extension methods
    /// </summary>
    /// <typeparam name="TSource">The type of the caching source</typeparam>
    public interface ICache<TSource> : ICache
    {
        /// <summary>
        /// Gets the object whose calls will be cached. typeof(TSource).Name will be used as the default region
        /// </summary>
        TSource Source { get;  }
    }
}
