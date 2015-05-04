using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A strategy to access or modify strongly-typed data in a Cache
    /// </summary>
    /// <typeparam name="T">The type of the data in the Cache</typeparam>
    public class CacheStrategy<T> : SingleValueCacheStrategy
    {
        internal CacheStrategy(ICache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        internal Func<ICachedValue<T>, CacheValidationResult> ValidateCallback { get; set; }
        internal Func<T> RetrieveCallback { get; set; }
        internal Func<Exception, ICachedValue<T>, RetrievalErrorHandlerResult<T>> RetrieveErrorHandler { get; set; }

        /// <summary>
        /// Gets the cached value wrapper from the cache
        /// </summary>
        /// <returns>The cached value wrapper</returns>
        public ICachedValue<T> Get()
        {
            ICachedValue<T> existingCachedValue = null;
            try
            {
                existingCachedValue = Cache.Get<T>(Key, Region);
            }
            catch (FluentCache.FluentCacheException cacheException)
            {
                if (!Cache.TryHandleCachingFailure(cacheException, CacheOperation.Get))
                    throw;
            }

            bool isValid = existingCachedValue != null;
            bool updateLastValidatedDate = true;

            if (existingCachedValue != null && ValidateCallback != null)
            {
                CacheValidationResult result = ValidateCallback(existingCachedValue);

                if (result == CacheValidationResult.Valid)
                {
                    updateLastValidatedDate = true;
                    isValid = true;
                }
                if (result == CacheValidationResult.Invalid)
                {
                    updateLastValidatedDate = false;
                    isValid = false;
                }
                else if (result == CacheValidationResult.Unknown)
                {
                    updateLastValidatedDate = false;
                    isValid = true;
                }
            }

            if (isValid && updateLastValidatedDate)
            {
                var markAsValidated = Cache as IUpdateLastValidatedDate;
                if (markAsValidated != null)
                    markAsValidated.MarkAsValidated(existingCachedValue, DateTime.UtcNow);
            }

            if (!isValid && RetrieveCallback != null)
            {
                T value;

                //note: we're going to only use a try/catch if we have an error handler, in order to avoid cluttering the stack trace
                if (RetrieveErrorHandler != null)
                {
                    try
                    {
                        value = RetrieveCallback();
                    }
                    catch (Exception x)
                    {
                        RetrievalErrorHandlerResult<T> errorHandlerResult = RetrieveErrorHandler(x, existingCachedValue);
                        if (errorHandlerResult != null && errorHandlerResult.IsErrorHandled)
                            value = errorHandlerResult.FallbackResult;
                        else
                            throw;
                    }
                }
                else
                {
                    value = RetrieveCallback();
                }

                try
                {
                    return Cache.Set<T>(Key, Region, value, CachePolicy);
                }
                catch(FluentCacheException x)
                {
                    if (!Cache.TryHandleCachingFailure(x, CacheOperation.Set))
                        throw;
                    else
                        return CachingFailedCachedValue.Create(value);
                }
            }
            else if (!isValid)
            {
                Cache.Remove(Key, Region);
                return null;
            }
            else
            {
                return existingCachedValue;
            }
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns false
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategy<T> InvalidateIf(Func<ICachedValue<T>, bool> validate)
        {
            Func<ICachedValue<T>, CacheValidationResult> val = existing => validate(existing) ? CacheValidationResult.Valid : CacheValidationResult.Invalid;
            return Validate(val);
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns CacheValidationResult.Invalid
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>

        public CacheStrategy<T> Validate(Func<ICachedValue<T>, CacheValidationResult> validate)
        {
            this.ValidateCallback = validate;
            return this;
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">A delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategy<T> RetrieveUsing(Func<T> retrieve)
        {
            this.RetrieveCallback = retrieve;
            return this;
        }

        /// <summary>
        /// Specifies an error handling strategy if retrieval fails
        /// </summary>
        /// <param name="errorHandler">An error handler that takes the exception and previous cached value (if it exists) and returns the fallback value</param>
        /// <returns>An updated cache strategy that includes the retrieval error handler strategy</returns>
        public CacheStrategy<T> IfRetrievalFails(Func<Exception, ICachedValue<T>, RetrievalErrorHandlerResult<T>> errorHandler)
        {
            this.RetrieveErrorHandler = errorHandler;
            return this;
        }

        /// <summary>
        /// Gets the cached value
        /// </summary>
        /// <returns>The cached value</returns>
        public T GetValue()
        {
            ICachedValue<T> cachedValue = this.Get();
            return cachedValue == null ? default(T) : cachedValue.Value;
        }

        /// <summary>
        /// Sets the cached value
        /// </summary>
        /// <param name="value">The value to be cached</param>
        public void SetValue(T value)
        {
            Cache.Set(Key, Region, value, CachePolicy);
        }

    }






}
