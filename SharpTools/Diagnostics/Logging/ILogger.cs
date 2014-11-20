using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpTools.Diagnostics.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Returns whether or not this logger is enabled for messages of the given logging level.
        /// </summary>
        /// <param name="level">The logging level to check</param>
        /// <returns>boolean</returns>
        bool IsEnabled(LogLevel level);
        /// <summary>
        /// Resets the logging level for this logger to the provided level.
        /// </summary>
        /// <param name="level">The new logging level</param>
        void SetLogLevel(LogLevel level);
        /// <summary>
        /// Enable logging of messages for the given logging level.
        /// </summary>
        /// <param name="level">The logging level to enable</param>
        void EnableLogLevel(LogLevel level);
        /// <summary>
        /// Disable logging of messages for the given logging level.
        /// </summary>
        /// <param name="level">The logging level to disable</param>
        void DisableLogLevel(LogLevel level);

        void Log(LogLevel level, object message);
        void Log(LogLevel level, object message, Exception exception);
        void Log(LogLevel level, string format, params object[] args);
        void Log(LogLevel level, IFormatProvider provider, string format, params object[] args);

        /// <summary>
        /// A queryable source for reading the logs
        /// </summary>
        IQueryable<string> Logs { get; }
    }

    public static class LoggerExtensions
    {
        // Log.Debug
        public static void Debug(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Debug, message);
        }
        public static void Debug(this ILogger logger, object message, Exception exception)
        {
            logger.Log(LogLevel.Debug, message, exception);
        }
        public static void Debug(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Debug, format, args);
        }
        public static void Debug(this ILogger logger, IFormatProvider provider, string format, params object[] args)
        {
            logger.Log(LogLevel.Debug, provider, format, args);
        }

        // Log.Info
        public static void Info(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Info, message);
        }
        public static void Info(this ILogger logger, object message, Exception exception)
        {
            logger.Log(LogLevel.Info, message, exception);
        }
        public static void Info(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Info, format, args);
        }
        public static void Info(this ILogger logger, IFormatProvider provider, string format, params object[] args)
        {
            logger.Log(LogLevel.Info, provider, format, args);
        }

        // Log.Warn
        public static void Warn(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Warn, message);
        }
        public static void Warn(this ILogger logger, object message, Exception exception)
        {
            logger.Log(LogLevel.Warn, message, exception);
        }
        public static void Warn(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Warn, format, args);
        }
        public static void Warn(this ILogger logger, IFormatProvider provider, string format, params object[] args)
        {
            logger.Log(LogLevel.Warn, provider, format, args);
        }

        // Log.Error
        public static void Error(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Error, message);
        }
        public static void Error(this ILogger logger, object message, Exception exception)
        {
            logger.Log(LogLevel.Error, message, exception);
        }
        public static void Error(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Error, format, args);
        }
        public static void Error(this ILogger logger, IFormatProvider provider, string format, params object[] args)
        {
            logger.Log(LogLevel.Error, provider, format, args);
        }

        // Log.Fatal
        public static void Fatal(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Fatal, message);
        }
        public static void Fatal(this ILogger logger, object message, Exception exception)
        {
            logger.Log(LogLevel.Fatal, message, exception);
        }
        public static void Fatal(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Fatal, format, args);
        }
        public static void Fatal(this ILogger logger, IFormatProvider provider, string format, params object[] args)
        {
            logger.Log(LogLevel.Fatal, provider, format, args);
        }
    }
}
