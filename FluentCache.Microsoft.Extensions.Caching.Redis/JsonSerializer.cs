using FluentCache.Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCache.Microsoft.Extensions.Caching.Redis
{
    /// <summary>
    /// ISerializer implementation using JSON
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <summary>
        /// Deserializes a value from JSON
        /// </summary>
        public T Deserialize<T>(string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Serializes a value to JSON
        /// </summary>
        public string Serialize(object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
    }
}
