using GenericCacheRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Services
{
    public class LoggerService : ILoggerService
    {
        public void LogInfo(string message) => Console.WriteLine($"INFO: {message}");
        public void LogError(string message, Exception ex) => Console.WriteLine($"ERROR: {message}, Exception: {ex.Message}");
        public void LogPerformance(string method, TimeSpan duration) => Console.WriteLine($"PERFORMANCE: {method} executed in {duration.TotalMilliseconds}ms");
    }

}
