using Microsoft.Extensions.Logging;

namespace Ferro.Common
{
    public class GlobalLogger
    {
        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}
