using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCache.Microsoft.Extensions.Caching.Distributed
{
    /// <summary>
    /// Defines an interface for serializing and deserializing values to the distributed cache
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize the specified object to a string
        /// </summary>
        string Serialize(object value);

        /// <summary>
        /// Deserialize the specified object from a string
        /// </summary>
        T Deserialize<T>(string value);
    }
}
