using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Test
{
    public class SimpleCachedValue : ICachedValue
    {
        public SimpleCachedValue()
        {
            AccessCount = 1;
        }
        public DateTime? ExpireAbsolute { get; internal set; }
        public DateTime? LastAccessed { get; internal set; }
        public TimeSpan? ExpireAfter { get; internal set; }
        public DateTime CachedDate { get; internal set; }
        public object Value { get; internal set; }
        public int AccessCount { get; private set; }

        internal bool CheckAccess(DateTime now)
        {
            if (ExpireAbsolute != null && now >= ExpireAbsolute)
                return true;
            else if (ExpireAfter != null && (now - LastAccessed) > ExpireAfter)
                return true;

            AccessCount++;
            LastAccessed = now;
            return false;
        }

        public ICachedValue<T> Wrap<T>()
        {
            return new SimpleCachedValueWrapper<T>(this);
        }


        public DateTime? LastValidatedDate { get; set; }
    }
}
