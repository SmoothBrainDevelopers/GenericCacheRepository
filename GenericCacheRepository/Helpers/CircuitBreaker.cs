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
        private int _failureThreshold = 3;
        private DateTime _lastFailureTime;
        private int _timeout = 60;

        public void SetTimeout(int timeoutSeconds)
        {
            _timeout = timeoutSeconds;
        }

        public bool AllowRequest()
        {
            if (_failureCount >= _failureThreshold)
            {
                if (DateTime.UtcNow - _lastFailureTime > TimeSpan.FromSeconds(_timeout))
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
