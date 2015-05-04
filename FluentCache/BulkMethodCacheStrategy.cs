using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{


    internal class BulkMethodCacheStrategy<TKey, TResult> : BulkCacheStrategyIncomplete<TKey, TResult>
    {
        internal BulkMethodCacheStrategy(ICache cache, Func<ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>> bulkGetMethod, string baseKey, ICollection<TKey> keys)
            : base(cache, baseKey, keys)
        {
            Method = bulkGetMethod;
        }

        private readonly Func<ICollection<TKey>, ICollection<KeyValuePair<TKey, TResult>>> Method;

        public BulkCacheStrategy<TKey, TResult> RetrieveUsingMethod()
        {
            return this.RetrieveUsing(Method);
        }
    }

    internal class AsyncBulkMethodCacheStrategy<TKey, TResult> : BulkCacheStrategyIncomplete<TKey, TResult>
    {
        internal AsyncBulkMethodCacheStrategy(ICache cache, Func<ICollection<TKey>, Task<ICollection<KeyValuePair<TKey, TResult>>>> bulkGetMethod, string baseKey, ICollection<TKey> keys)
            : base(cache, baseKey, keys)
        {
            Method = bulkGetMethod;
        }

        private readonly Func<ICollection<TKey>, Task<ICollection<KeyValuePair<TKey, TResult>>>> Method;

        public BulkCacheStrategyAsync<TKey, TResult> RetrieveUsingMethod()
        {
            return this.RetrieveUsingAsync(Method);
        }
    }


   


}
