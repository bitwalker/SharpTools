using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpTools.Collections
{
    public sealed class RingBuffer<T> : IEnumerable<T>
    {
        private readonly object _lock = new object();
        private readonly T[] _buffer;
        private readonly int _upperBound;
        private int _cursor;
        
        public int BufferSize { get; private set; }

        public RingBuffer(int bufferSize)
        {
            BufferSize = bufferSize;
            _buffer = new T[BufferSize];
            _upperBound = _buffer.GetUpperBound(0);
            _cursor = 0;
        }

        public RingBuffer(int bufferSize, IEnumerable<T> items) : this(bufferSize)
        {
            foreach (var item in items)
            {
                Append(item);
            }
        }

        public void Append(T item)
        {
            lock (_lock)
            {
                if (_cursor > _upperBound)
                    _cursor = 0;

                _buffer[_cursor] = item;
                _cursor++;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index > _upperBound)
                    return _buffer[index - (_upperBound - index)];
                else
                    return _buffer[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new RingBufferEnumerator<T>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new RingBufferEnumerator<T>(this);
        }

        public class RingBufferEnumerator<T> : IEnumerator<T>
        {
            private readonly RingBuffer<T> _buffer;
            private int _cursor;

            public RingBufferEnumerator(RingBuffer<T> buffer)
            {
                _buffer = buffer;
                _cursor = 0;
            }

            public T Current
            {
                get { return _buffer[_cursor]; }
            }

            public void Dispose()
            {
                _cursor = 0;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _buffer[_cursor]; }
            }

            public bool MoveNext()
            {
                if (_cursor >= _buffer._upperBound)
                    return false;

                _cursor++;

                return true;
            }

            public void Reset()
            {
                _cursor = 0;
            }
        }

    }
}
