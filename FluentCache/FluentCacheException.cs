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
        /// Constructs a new instance
        /// </summary>
        public  FluentCacheException() { }
        
        /// <summary>
        /// Constructs a new instance with the specified message
        /// </summary>
        public  FluentCacheException(string message) : base(message) { }
        
        /// <summary>
        /// Constructs a new instance with the specified message and inner exception
        /// </summary>
        public  FluentCacheException(string message, Exception inner) : base(message, inner) { }

    }
}
