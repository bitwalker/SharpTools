using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using SharpTools.Concurrency;
using SharpTools.Extend.Time;
using SharpTools.Extend.Collections;

namespace SharpTools.Collections
{
    public sealed class RingBuffer<T> : IEnumerable<T>
    {
        private static readonly TimeSpan _writeLockTimeout = 50.Milliseconds();

        private readonly ReaderWriterLockSlim _lock;
        private readonly bool _throwOnFailedWrite;

        private T[] _buffer;
        private int _bufferLength;
        private int _upperBound;
        private int _cursor;
        
        public int BufferSize { get { return _bufferLength; } }
        public int UpperBound { get { return _upperBound; } }

        /// <summary>
        /// Creates a new <see cref="RingBuffer{T}"/>.
        /// </summary>
        /// <param name="bufferSize">The size of the RingBuffer</param>
        /// <param name="throwOnFailedWrite">If true, will throw a </param>
        public RingBuffer(int bufferSize, bool throwOnFailedWrite = false)
        {
            _lock = new ReaderWriterLockSlim();
            _throwOnFailedWrite = throwOnFailedWrite;

            Reset(bufferSize);
        }

        public RingBuffer(int bufferSize, IEnumerable<T> items, bool throwOnFailedWrite)
            : this(bufferSize, throwOnFailedWrite)
        {
            var buffer = items.ToArray();
            if (buffer.Length > _bufferLength)
            {
                _buffer = buffer.Slice(buffer.Length - _bufferLength);
            }
            else
            {
                _buffer = buffer.Slice(0);
            }
        }

        public void Append(T item)
        {
            // Discard this log message if we can't obtain a write lock, unless throwOnFailedWrite=true
            if (_lock.TryEnterWriteLock(_writeLockTimeout))
            {
                try
                {
                    if (_cursor > _upperBound)
                        _cursor = 0;

                    _buffer[_cursor] = item;
                    _cursor++;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            else
            {
                if (!_throwOnFailedWrite)
                    return;
                else
                    throw new WriteLockException("Failed to acquire write lock after {0}ms in RingBuffer.Append!", _writeLockTimeout.ToString("fff"));
            }
        }

        public void SetBufferSize(int bufferSize)
        {
            _lock.EnterWriteLock();

            try
            {
                Reset(bufferSize);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();

                try
                {
                    if (index > _upperBound)
                        return _buffer[index - (_upperBound - index)];
                    else
                        return _buffer[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new RingBufferEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RingBufferEnumerator<T>(this);
        }
    
        private void Reset(int bufferSize)
        {
            _bufferLength = bufferSize;
            _buffer       = null;
            _buffer       = new T[_bufferLength];
            _upperBound   = _buffer.GetUpperBound(0);
            _cursor       = 0;
        }

        ~RingBuffer()
        {
            if (_lock != null)
                _lock.Dispose();
        }
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
            if (_cursor >= _buffer.BufferSize)
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
