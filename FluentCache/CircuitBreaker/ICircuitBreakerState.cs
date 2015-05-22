using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.CircuitBreaker
{
    /// <summary>
    /// Defines a simple circuit breaker. See http://martinfowler.com/bliki/CircuitBreaker.html
    /// </summary>
    public interface ICircuitBreakerState
    {
        /// <summary>
        /// Gets the last exception that caused the circuit to break
        /// </summary>
        Exception LastException { get; }

        /// <summary>
        /// Gets a value that indicates whether the current circuit is broken
        /// </summary>
        bool IsBroken { get; }

        /// <summary>
        /// Resets the circuit to an open state
        /// </summary>
        void Reset();

        /// <summary>
        /// Attempt to break the circuit
        /// </summary>
        void TryBreak(Exception ex);
    }
}
