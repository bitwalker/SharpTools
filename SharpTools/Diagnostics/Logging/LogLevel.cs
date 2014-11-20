using System;

namespace SharpTools.Diagnostics.Logging
{
    [Flags]
    public enum LogLevel
    {
        Disabled = -1,
        Debug    = 1,
        Info     = 1 << 1,
        Warn     = 1 << 2,
        Error    = 1 << 3,
        Fatal    = 1 << 4,
        All      = Debug | Info | Warn | Error | Fatal
    }
}
