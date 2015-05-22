using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// Defines the result of a retrieval error handler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RetrievalErrorHandlerResult<T>
    {
        /// <summary>
        /// Indicates whether the retrieval error was handled
        /// </summary>
        public bool IsErrorHandled { get; set; }

        /// <summary>
        /// Specifies the fallback value that should be used
        /// </summary>
        public T FallbackResult { get; set; }

        /// <summary>
        /// A simple retrieval error handler that uses the previous cached value, if it exists
        /// </summary>
        public static RetrievalErrorHandlerResult<T> UsePreviousCachedValue(Exception retrievalException, CachedValue<T> previousValue)
        {
            if (previousValue != null)
                return new RetrievalErrorHandlerResult<T> { FallbackResult = previousValue.Value, IsErrorHandled = true };
            else
                return new RetrievalErrorHandlerResult<T> { IsErrorHandled = false };
        }

        /// <summary>
        /// A simple retrieval error handler that uses the previous cached value if it exists, otherwise uses a default value
        /// </summary>
        public static RetrievalErrorHandlerResult<T> UsePreviousCachedValueOrDefault(Exception retrievalException, CachedValue<T> previousValue, T defaultValue)
        {
            return new RetrievalErrorHandlerResult<T> { IsErrorHandled = true, FallbackResult = previousValue == null? defaultValue : previousValue.Value };
        } 
    }
}
