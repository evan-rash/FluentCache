using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.Execution
{
    /// <summary>
    /// Defines an execution plan for getting a value from a cache, validating, and retrieving a new value
    /// </summary>
    /// <typeparam name="T">The type of value that is cached</typeparam>
    public interface ICacheExecutionPlan<T>
    {
        /// <summary>
        /// Executes the plan
        /// </summary>
        CachedValue<T> Execute();

        /// <summary>
        /// Asynchronously executes the plan
        /// </summary>
        Task<CachedValue<T>> ExecuteAsync();
    }
}
