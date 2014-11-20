using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SharpTools.Testing.EntityFramework.Internal.Id
{
    [DebuggerDisplay("{_counter}", Name = "AutoIncrementingShortIdentifierGenerator")]
    internal sealed class AutoIncrementingShortIdentifierGenerator : IIdentifierGenerator
    {
        private readonly object _lock = new object();
        private short _counter;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Generate()
        {
            lock (_lock)
            {
                if (_counter < 0)
                    _counter = 0;

                _counter++;
                return _counter;
            }
        }
    }

    [DebuggerDisplay("{_counter}", Name = "AutoIncrementingIntegerIdentifierGenerator")]
    internal sealed class AutoIncrementingIntegerIdentifierGenerator : IIdentifierGenerator
    {
        private readonly object _lock = new object();
        private int _counter;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Generate()
        {
            lock (_lock)
            {
                if (_counter < 0)
                    _counter = 0;

                _counter++;
                return _counter;
            }
        }
    }

    [DebuggerDisplay("{_counter}", Name = "AutoIncrementingLongIdentifierGenerator")]
    internal sealed class AutoIncrementingLongIdentifierGenerator : IIdentifierGenerator
    {
        private readonly object _lock = new object();
        private long _counter;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Generate()
        {
            lock (_lock)
            {
                if (_counter < 0)
                    _counter = 0;

                _counter++;
                return _counter;
            }
        }
    }
}
