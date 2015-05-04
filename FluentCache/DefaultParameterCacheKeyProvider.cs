using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// DefaultParameterCacheKeyProvider provides a simple and standard mechanism for generating parameter cache key values
    /// </summary>
    public class DefaultParameterCacheKeyProvider : IParameterCacheKeyProvider
    {
        /// <summary>
        /// Generates a cache key value for the specified parameter by using parameter.ToString()
        /// </summary>
        public virtual string GenerateParameterCacheKey(object parameter)
        {
            if (parameter == null)
                return String.Empty;
            else
                return String.Format("{0}", parameter);
        }
    }
}
