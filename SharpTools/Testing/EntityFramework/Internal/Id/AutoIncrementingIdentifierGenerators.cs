using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SharpTools.Testing.EntityFramework.Internal.Id
{
    [DebuggerDisplay("{_counter}", Name = "AutoIncrementingShortIdentifierGenerator")]
    internal sealed class AutoIncrementingShortIdentifierGenerator : IIdentifierGenerator
    {
        private short _counter;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Generate()
        {
            if (_counter < 0)
                _counter = 0;

            _counter++;
            return _counter;
        }
    }

    [DebuggerDisplay("{_counter}", Name = "AutoIncrementingIntegerIdentifierGenerator")]
    internal sealed class AutoIncrementingIntegerIdentifierGenerator : IIdentifierGenerator
    {
        private int _counter;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Generate()
        {
            if (_counter < 0)
                _counter = 0;

            _counter++;
            return _counter;
        }
    }

    [DebuggerDisplay("{_counter}", Name = "AutoIncrementingLongIdentifierGenerator")]
    internal sealed class AutoIncrementingLongIdentifierGenerator : IIdentifierGenerator
    {
        private long _counter;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Generate()
        {
            if (_counter < 0)
                _counter = 0;

            _counter++;
            return _counter;
        }
    }
}
