using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// The base for exceptions thrown by the FluentCache APIs
    /// </summary>
    public class FluentCacheException : Exception
    {
        /// <summary>
        /// Constructs a new instance with the specified message, operation, and inner exception
        /// </summary>
        public  FluentCacheException(string message, CacheOperation operation, Exception innerException) : base(message, innerException)
        {
            Operation = operation;
        }

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public FluentCacheException()
            : base()
        {
        }

        /// <summary>
        /// Constructs a new instance with the specified message
        /// </summary>
        public FluentCacheException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs a new instance with the specified message and inner exception
        /// </summary>
        public FluentCacheException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Specifies which caching operation failed
        /// </summary>
        public CacheOperation? Operation { get; set; }

    }
}
