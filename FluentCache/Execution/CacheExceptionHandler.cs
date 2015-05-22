using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Execution
{
    /// <summary>
    /// Helpers to create cache exception handlers
    /// </summary>
    public class CacheExceptionHandler : ICacheExceptionHandler
    {
        /// <summary>
        /// Gets the default cache exception handler
        /// </summary>
        public static readonly ICacheExceptionHandler Default = new CacheExceptionHandler(null);

        /// <summary>
        /// Creates a cache exception handler from the specified predicate
        /// </summary>
        public static ICacheExceptionHandler FromPredicate(Func<FluentCacheException, bool> handleException)
        {
            return new CacheExceptionHandler(handleException);
        }

        private CacheExceptionHandler(Func<FluentCacheException, bool> handleException)
        {
            HandleException = handleException;
        }

        private readonly Func<FluentCacheException, bool> HandleException;

        bool ICacheExceptionHandler.TryHandleCachingFailure(FluentCacheException cacheException)
        {
            return HandleException != null && HandleException(cacheException);
        }
    }
}
