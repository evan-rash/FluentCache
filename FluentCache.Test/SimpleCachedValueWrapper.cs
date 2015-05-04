using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    public class SimpleCachedValueWrapper<T> : ICachedValue<T>
    {
        public SimpleCachedValueWrapper(SimpleCachedValue v)
        {
            CachedValue = v;
        }
        public SimpleCachedValue CachedValue { get; private set; }

        public T Value { get { return (T)CachedValue.Value; } }

        object ICachedValue.Value { get { return Value; } }

        public DateTime CachedDate { get { return CachedValue.CachedDate; } }


        public DateTime? LastValidatedDate { get { return CachedValue.LastValidatedDate; } }
    }

}
