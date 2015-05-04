using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Defines how cache strategies should generate caching keys for parameter values
    /// </summary>
    public interface IParameterCacheKeyProvider
    {
        /// <summary>
        /// Generate a cache key for the specified parameter
        /// </summary>
        /// <returns>a string representation of the parameter that is used in generating a unique cache key for a cache strategy</returns>
        string GenerateParameterCacheKey(object parameter);
    }
}
