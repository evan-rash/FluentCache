using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Default methods for generating cache keys for parameters
    /// </summary>
    public static class ParameterCacheKeys
    {
        /// <summary>
        /// Generates a cache key for a parameter value. The default implementation uses parameter.ToString()
        /// </summary>
        public static string GenerateCacheKey(object parameter)
        {
            if (parameter == null)
                return String.Empty;

            else if (parameter is string)
                return (string)parameter;
            else if (parameter is IEnumerable)
                return GenerateCacheKey(parameter as IEnumerable);
            else
                return parameter.ToString();
        }

        /// <summary>
        /// Generates a cache key for an enumerable parameter value
        /// </summary>
        public static string GenerateCacheKey(IEnumerable parameter)
        {
            if (parameter == null)
                return String.Empty;
            else
                return GenerateCacheKey(parameter.Cast<object>());
        }

        /// <summary>
        /// Generates a cache key for an enumerable parameter value
        /// </summary>
        public static string GenerateCacheKey(IEnumerable<object> parameter)
        {
            if (parameter == null)
                return String.Empty;
            else
                return "[" + String.Join(",", parameter) + "]";
        }
    }
}
