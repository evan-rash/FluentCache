using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Execution
{
    /// <summary>
    /// Handles exceptions that occur during caching
    /// </summary>
    public interface ICacheExceptionHandler
    {
        /// <summary>
        /// Attempts to handle a failure that occured during caching. Return true to indicate that the error was handled and false to indicate the error is unhandled and needs to be propogated further
        /// </summary>
        /// <param name="cacheException">The exception that occurred during caching</param>
        /// <returns>true to indicate the error was handled, false to indicate it is unhandled</returns>
        bool TryHandleCachingFailure(FluentCacheException cacheException);

    }
}
