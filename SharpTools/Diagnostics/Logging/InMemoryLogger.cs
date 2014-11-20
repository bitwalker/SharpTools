using System;
using System.Linq;
using SharpTools.Collections;

namespace SharpTools.Diagnostics.Logging
{
    public class InMemoryLogger : ILogger
    {
        // Max 10mb used for storing the in memory logs
        private const int MAX_MEMORY_USAGE = 10*1024*1024;
        // An array of all the log levels for easy use
        private static readonly LogLevel[] LEVELS = Enum.GetValues(typeof (LogLevel)) as LogLevel[];

        private readonly RingBuffer<string> _log;
        private LogLevel _level;

        public InMemoryLogger(LogLevel logLevel = LogLevel.All)
        {
            _level = logLevel;
            _log   = new RingBuffer<string>(MAX_MEMORY_USAGE);
        }

        public IQueryable<string> Logs
        {
            get { return _log.AsQueryable(); }
        }

        public bool IsEnabled(LogLevel level)
        {
            return ((int) level) >= ((int) _level);
        }

        public void SetLogLevel(LogLevel level)
        {
            _level = level;
        }

        public void EnableLogLevel(LogLevel level)
        {
            if (_level.HasFlag(LogLevel.Disabled))
            {
                _level = level;
            }
            else if (!_level.HasFlag(level))
            {
                _level = _level | level;
            }
        }

        public void DisableLogLevel(LogLevel level)
        {
            if (_level.HasFlag(LogLevel.Disabled))
            {
                _level = LogLevel.All;
            }
            else
            {
                _level = LEVELS
                    .Where(l => _level.HasFlag(l) && !level.HasFlag(l))
                    .Aggregate((a, b) => a | b);
            }
        }

        public void Log(LogLevel level, object message)
        {
            var timestamp = DateTime.UtcNow.ToString("s");
            var levelName = Enum.GetName(typeof (LogLevel), level);
            _log.Append(string.Format("{0}  {1} {2}", timestamp, levelName, message));
        }

        public void Log(LogLevel level, object message, Exception exception)
        {
            var timestamp  = DateTime.UtcNow.ToString("s");
            var levelName  = Enum.GetName(typeof (LogLevel), level);
            var exTypeName = exception.GetType().Name;
            var exMessage  = exception.Message;
            _log.Append(string.Format("{0}  {1} {2} - {3}:{4}", timestamp, levelName, message, exTypeName, exMessage));
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            var timestamp  = DateTime.UtcNow.ToString("s");
            var levelName  = Enum.GetName(typeof (LogLevel), level);
            _log.Append(string.Format("{0} {1} {2}", timestamp, levelName, string.Format(format, args)));
        }

        public void Log(LogLevel level, IFormatProvider provider, string format, params object[] args)
        {
            var timestamp = DateTime.UtcNow.ToString("s");
            var levelName = Enum.GetName(typeof (LogLevel), level);
            var formatted = string.Format(provider, format, args);
            _log.Append(string.Format("{0} {1} {2}", timestamp, levelName, formatted));
        }
    }
}
