using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Expressions
{
    /// <summary>
    /// An exception that is thrown when the fluent caching API is unable to parse the provided expression
    /// </summary>
    public class InvalidCachingExpressionException : FluentCacheException
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public InvalidCachingExpressionException() { }

        /// <summary>
        /// Constructs a new instance with the specified message
        /// </summary>
        public InvalidCachingExpressionException(string message) : base(message) { }

        /// <summary>
        /// Constructs a new instance with the specified message and inner exception
        /// </summary>
        public InvalidCachingExpressionException(string message, Exception inner) : base(message, inner) { }
    }
}
