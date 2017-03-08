using Microsoft.Extensions.Logging;

namespace Ferro.Common
{
    public class GlobalLogger
    {
        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }

    // exposes a set of protocol codes that we can pass to the Logger
    // for display.
    public struct LoggingEvents
    {
        // DHT PROTOCOL
        public const int DHT_PROTOCOL_MSG   = 1000;
        public const int ATTEMPT_CONNECTION = 1001;

        public const int DHT_ERROR = 1500;

        // PEER PROTOCOL 
        public const int PEER_PROTOCOL_MSG        = 2000; // otherwise unclassified
        public const int HANDSHAKE_OUTGOING       = 2001;
        public const int HANDSHAKE_INCOMING       = 2002;
        public const int EXTENSION_HEADER_OUT     = 2003;
        public const int EXTENSION_HEADER_IN      = 2004;
        public const int EXTENSION_MESSAGE        = 2005;
        public const int PEER_UNEXPECTED_RESPONSE = 2009;

        public const int METADATA_EXCHANGE          = 2010;
        public const int METADATA_REQUEST           = 2011;
        public const int METADATA_RESPONSE_INCOMING = 2012;
        public const int METADATA_RESPONSE_OUTGOING = 2013;
        public const int METADATA_FAILURE           = 2019;

        // DATA STORAGE
        public const int DATA_STORAGE_ACTION = 5000;
    }
}
