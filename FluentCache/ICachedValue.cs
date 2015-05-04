using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A value that has been retrieved from a cache
    /// </summary>
    public interface ICachedValue 
    {
        /// <summary>
        /// Gets the date the value was first cached
        /// </summary>
        DateTime CachedDate { get; }

        /// <summary>
        /// Gets the date the cached value was last validated, or null if it has not been validated
        /// </summary>
        DateTime? LastValidatedDate { get;  }

        /// <summary>
        /// Gets the cached value
        /// </summary>
        object Value { get; }
    }




}
