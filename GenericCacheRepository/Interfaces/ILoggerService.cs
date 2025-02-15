using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Interfaces
{
    public interface ILoggerService
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex);
        void LogPerformance(string method, TimeSpan duration);
    }

}
