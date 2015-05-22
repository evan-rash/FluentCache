using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCache.CircuitBreaker
{
    /// <summary>
    /// A simple implementation of a circuit breaker that breaks after a specified number of failures for the specified duration
    /// </summary>
    public sealed class CircuitBreakerState : ICircuitBreakerState
    {
        private readonly TimeSpan _durationOfBreak;
        private readonly int _exceptionsAllowedBeforeBreaking;
        private int _count;
        private DateTime _blockedTill;
        private Exception _lastException;
        private readonly object _lock = new object();

        /// <summary>
        /// Constructs a new instance of the circuit breaker
        /// </summary>
        /// <param name="exceptionsAllowedBeforeBreaking">The number of exceptions to allow before breaking the circuit</param>
        /// <param name="durationOfBreak">The duration that the circuit should remain broken</param>
        public CircuitBreakerState(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            _durationOfBreak = durationOfBreak;
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;

            Reset();
        }

        /// <summary>
        /// Gets the last exception that caused the circuit to break
        /// </summary>
        public Exception LastException
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    return _lastException;
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the circuit is broken
        /// </summary>
        public bool IsBroken
        {
            get
            {
                using (TimedLock.Lock(_lock))
                {
                    return DateTime.UtcNow < _blockedTill;
                }
            }
        }

        /// <summary>
        /// Resets the circuit to an open state
        /// </summary>
        public void Reset()
        {
            using (TimedLock.Lock(_lock))
            {
                _count = 0;
                _blockedTill = DateTime.MinValue;

                _lastException = new InvalidOperationException("This exception should never be thrown");
            }
        }

        /// <summary>
        /// Attempts to break the circuit
        /// </summary>
        public void TryBreak(Exception ex)
        {
            using (TimedLock.Lock(_lock))
            {
                _lastException = ex;

                _count += 1;
                if (_count >= _exceptionsAllowedBeforeBreaking)
                {
                    BreakTheCircuit();
                }
            }
        }

        void BreakTheCircuit()
        {
            var willDurationTakeUsPastDateTimeMaxValue = _durationOfBreak > DateTime.MaxValue - DateTime.UtcNow;
            _blockedTill = willDurationTakeUsPastDateTimeMaxValue ?
                               DateTime.MaxValue :
                               DateTime.UtcNow + _durationOfBreak;
        }
    }
}
