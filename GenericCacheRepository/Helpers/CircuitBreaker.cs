using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Helpers
{
    public class CircuitBreaker
    {
        private int _failureCount = 0;
        private readonly int _failureThreshold = 3;
        private DateTime _lastFailureTime;

        public bool AllowRequest()
        {
            if (_failureCount >= _failureThreshold)
            {
                if (DateTime.UtcNow - _lastFailureTime > TimeSpan.FromMinutes(1))
                {
                    _failureCount = 0;
                    return true;
                }
                return false;
            }
            return true;
        }

        public void RecordFailure()
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
        }
    }

}
