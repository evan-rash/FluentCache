using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache
{
    /// <summary>
    /// A strategy to asynchronously access or modify strongly-typed data in a Cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheStrategyAsync<T> : SingleValueCacheStrategy
    {
        internal CacheStrategyAsync(ICache cache, string baseKey)
            : base(cache, baseKey)
        {
        }

        internal Func<ICachedValue<T>, Task<CacheValidationResult>> ValidateCallback { get; set; }
        internal Func<Task<T>> RetrieveCallback { get; set; }
        internal Func<Exception, ICachedValue<T>, RetrievalErrorHandlerResult<T>> RetrieveErrorHandler { get; set; }

        /// <summary>
        /// Asynchronously gets the cached value wrapper from the cache
        /// </summary>
        /// <returns>A task containing the cached value wrapper</returns>
        public async Task<ICachedValue<T>> GetAsync()
        {
            ICachedValue<T> existingCachedValue = null;

            try
            {
                existingCachedValue = Cache.Get<T>(Key, Region);
            }
            catch(FluentCacheException cacheException)
            {
                if (!Cache.TryHandleCachingFailure(cacheException, CacheOperation.Get))
                    throw;
            }

            bool isValid = existingCachedValue != null;
            bool updateLastValidatedDate = true;
            
            if (existingCachedValue != null && ValidateCallback != null)
            {
                CacheValidationResult result = await ValidateCallback(existingCachedValue);
                
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
                        value = await RetrieveCallback();
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
                    value = await RetrieveCallback();
                }

                try
                {
                    return Cache.Set<T>(Key, Region, value, CachePolicy);
                }
                catch (FluentCacheException cacheException)
                {
                    if (!Cache.TryHandleCachingFailure(cacheException, CacheOperation.Set))
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
        public CacheStrategyAsync<T> InvalidateIf(Func<ICachedValue<T>, bool> validate)
        {
            Func<ICachedValue<T>, Task<bool>> val = existing => Task.FromResult(validate(existing));
            return InvalidateIfAsync(val);
        }

        /// <summary>
        /// Invalidates the cached value if the specified asynchronous validation delegate returns false
        /// </summary>
        /// <param name="validate">An asynchronous delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategyAsync<T> InvalidateIfAsync(Func<ICachedValue<T>, Task<bool>> validate)
        {
            Func<ICachedValue<T>, Task<CacheValidationResult>> val = async existing => (await validate(existing))? CacheValidationResult.Valid : CacheValidationResult.Invalid;
            return ValidateAsync(val);
        }

        /// <summary>
        /// Invalidates the cached value if the specified asynchronous validation delegate returns CacheValidationResult.Invalid
        /// </summary>
        /// <param name="validate">An asynchronous delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategyAsync<T> ValidateAsync(Func<ICachedValue<T>, Task<CacheValidationResult>> validate)
        {
            this.ValidateCallback = validate;
            return this;
        }

        /// <summary>
        /// Invalidates the cached value if the specified validation delegate returns CacheValidationResult.Invalid
        /// </summary>
        /// <param name="validate">A delegate that validates the cached value</param>
        /// <returns>An updated cache strategy that includes the invalidation strategy</returns>
        public CacheStrategyAsync<T> Validate(Func<ICachedValue<T>, CacheValidationResult> validate)
        {
            Func<ICachedValue<T>, Task<CacheValidationResult>> val = existing => Task.FromResult(validate(existing));
            return ValidateAsync(val);
        }

        /// <summary>
        /// Specifies a data retrieval strategy if the desired value does not exist in the cache or is invalid
        /// </summary>
        /// <param name="retrieve">An asynchronous delegate that specifies how the value is to be retrieved</param>
        /// <returns>An updated cache strategy that includes the retrieval strategy</returns>
        public CacheStrategyAsync<T> RetrieveUsingAsync(Func<Task<T>> retrieve)
        {
            this.RetrieveCallback = retrieve;
            return this;
        }

        /// <summary>
        /// Specifies an error handling strategy if retrieval fails
        /// </summary>
        /// <param name="errorHandler">An error handler that takes the exception and previous cached value (if it exists) and returns the fallback value</param>
        /// <returns>An updated cache strategy that includes the retrieval error handler strategy</returns>
        public CacheStrategyAsync<T> IfRetrievalFails(Func<Exception, ICachedValue<T>, RetrievalErrorHandlerResult<T>> errorHandler)
        {
            this.RetrieveErrorHandler = errorHandler;
            return this;
        }

        /// <summary>
        /// Asynchronously gets the cached value
        /// </summary>
        /// <returns>A task containing the cached value</returns>
        public async Task<T> GetValueAsync()
        {
            ICachedValue<T> cachedValue = await this.GetAsync();
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
