using System;
using System.Threading.Tasks;

namespace SharpTools.IO.Utility
{
    /// <summary>
    /// Provides asynchronous methods for watching for file changes.
    /// </summary>
    public interface IFileWatcher
    {
        /// <summary>
        /// Waits until a file is ready for use (created, not locked/shared).
        /// If the file is ready when called, it will immediately return, otherwise
        /// it will block until the file becomes ready, or until the timeout
        /// is reached, whichever occurs first.
        /// </summary>
        /// <param name="file">The full file path of the file to wait for.</param>
        /// <param name="timeout">The amount of time to wait for the file to become ready. Defaults to infinity.</param>
        /// <exception cref="TimeoutException">Thown if the timeout is reached before the file is ready.</exception>
        /// <returns>An awaitable task.</returns>
        Task WaitFor(string file, TimeSpan? timeout = null);
    }
}