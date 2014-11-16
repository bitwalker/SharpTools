using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Permissions;
using System.Threading.Tasks;

using SharpTools.Extend.Time;

namespace SharpTools.IO.Utility
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class FileWatcher : IFileWatcher
    {
        public async Task WaitFor(string file, TimeSpan? timeout = null)
        {
            using (var observer = new FileObserver(file))
            {
                await observer.WaitForReady(timeout);
            }
        }

        /// <summary>
        /// FileObserver works by monitoring a file for events, and checking for availability after each
        /// event occurs. FileSystemWatcher is unreliable on it's own, as the order of events changes based
        /// on not only how the file was created, but what created it. Due to this, you can't just wait for
        /// the Created event, because it may come before the file is actually written, or after. The Changed
        /// event is fired many times during file creation, so waiting for that is not viable either. Instead,
        /// the most foolproof method seems to be to pipe those events through an observable sequence, throttle
        /// it by some period, and then after each event occurs, check to see if the file is still locked. If it
        /// is, then we know we need to wait for more events to occur, if not, then we know the file was not only
        /// created, but fully written as well. The following is a summary of how FileObserver works in more detail:
        /// 
        /// On instantiation:
        /// 
        /// - FileSystemWatcher is configured to watch the provided file path
        /// - Raising of events by the watcher is left disabled
        /// - OnFileSystemEvent is registered as the event handler for FileSystemWatcher.Created/Changed,
        ///   which handles converting the WatcherChangeTypes to our own Event enumeration,
        ///   as well as publishing them via the _events observable.
        ///
        /// WaitForReady is called:
        ///
        /// - If the file is ready right away, nothing else is done, and WaitForReady returns
        /// - OnChange is subscribed to the _events observable, and throttled to 50ms to prevent
        ///   opening streams guaranteed to fail. It filters the events to look only for Created
        ///   and Changed.
        /// - Each time OnChange is called, we check to see if the file is available (meaning it's not open
        ///   elsewhere, and not locked). If it's available, the Ready event is published.
        /// - Raising of events is enabled on the FileSystemWatcher.
        /// - OnReady is subscribed to the _events observable, with the provided timeout (or an infinite one
        ///   if not), and filtered to look only for the Ready event.
        /// - WaitForReady then awaits completion of the OnReady subscription. This occurs one of two ways:
        ///   1. A TimeoutException is thrown, in which case that exception is returned up the stack
        ///   2. A Ready event is published, which results in OnReady being called. When OnReady is executed,
        ///      event raising is disabled on the FileSystemWatcher, and the observable is completed, resulting
        ///      in WaitForReady's return.
        /// </summary>
        private class FileObserver : IDisposable
        {
            private static readonly TimeSpan MAX_TIMEOUT = new TimeSpan(Int32.MaxValue - 1);

            private enum Event
            {
                Watching = 0,
                Created,
                Changed,
                Ready,
            }

            private const NotifyFilters NOTIFY_FILTERS =
                NotifyFilters.CreationTime |
                NotifyFilters.LastWrite |
                NotifyFilters.Size;


            private string _file;
            private bool _isDisposed;
            private FileSystemWatcher _watcher;
            private Subject<Event> _events;
            private IDisposable _eventObserver;

            public FileObserver(string file)
            {
                _file      = file;
                _watcher   = new FileSystemWatcher();
                _events    = new Subject<Event>();

                _watcher.Path         = Path.GetDirectoryName(_file);
                _watcher.Filter       = Path.GetFileName(_file);
                _watcher.NotifyFilter = NOTIFY_FILTERS;
                _watcher.Created += new FileSystemEventHandler(OnFileSystemEvent);
                _watcher.Changed += new FileSystemEventHandler(OnFileSystemEvent);
            }

            public async Task WaitForReady(TimeSpan? timeout)
            {
                if (_isDisposed)
                    throw new Exception("Attempted to use FileObserver after it was disposed!");

                if (!IsValidTimeout(timeout))
                    throw new ArgumentException("Invalid timeout value. Must be greater than TimeSpan.Zero, and less than TimeSpan(Int32.MaxValue)");

                // If the file is ready right away, just return
                if (File.Exists(_file) && !IsFileLocked(_file))
                    return;

                // Subscribe to Created and Changed events, but throttled by a short
                // period of time to prevent checking the file's availability when it's
                // already a given that it will fail. We will always get at least one event
                // when throttling. OnChange is bound as the handler for this subscription.
                _eventObserver = _events
                    .Where(ev => ev == Event.Created || ev == Event.Changed)
                    .Throttle(50.Milliseconds())
                    .Subscribe(OnChange);


                // Start watching
                _watcher.EnableRaisingEvents = true;

                // Wait until either a timeout occurs, or _events.OnCompleted is called,
                // which is done by OnReady when a Ready event is received.
                await _events
                    .Timeout(timeout.GetValueOrDefault(MAX_TIMEOUT))
                    .Where(ev => ev == Event.Ready)
                    .ForEachAsync(OnReady);
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                _watcher.EnableRaisingEvents = false;

                if (_eventObserver != null)
                    _eventObserver.Dispose();

                _watcher.Dispose();
                _watcher = null;

                _isDisposed = true;
            }

            private void OnFileSystemEvent(object state, FileSystemEventArgs e)
            {
                // Theoretically this could be extended to care about other events,
                // but for now this is all we care about.
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        _events.OnNext(Event.Created);
                        break;
                    case WatcherChangeTypes.Changed:
                        _events.OnNext(Event.Changed);
                        break;
                    default:
                        break;
                }
            }

            private void OnChange(Event e)
            {
                // Check for file availability
                if (!IsFileLocked(_file))
                    _events.OnNext(Event.Ready);
            }

            private void OnReady(Event e)
            {
                // Wrap it guys, we're done here
                if (_watcher.EnableRaisingEvents)
                    _watcher.EnableRaisingEvents = false;

                _events.OnCompleted();
            }

            private const int ERROR_SHARING_VIOLATION = 32;
            private const int ERROR_LOCK_VIOLATION = 33;
            private static bool IsFileLocked(string file)
            {
                FileStream stream = null;

                // Attempt to open the file with full permissions and no sharing allowed.
                // This ensures that not only can we open the file, but that nobody else has it open either.
                try
                {
                    stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException ex)
                {
                    // We could get exceptions for any number of reasons, but the file is only "locked"
                    // if one of the following lower-level error codes is given.
                    if (ex.HResult == ERROR_LOCK_VIOLATION || ex.HResult == ERROR_SHARING_VIOLATION)
                        return true;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }

                return false;
            }

            private static bool IsValidTimeout(TimeSpan? timeout)
            {
                // If no timeout was given, we use a predefined valid timeout
                if (!timeout.HasValue)
                    return true;

                var span = timeout.Value;
                if (span < TimeSpan.Zero)
                    return false;

                if (span > MAX_TIMEOUT)
                    return false;

                return true;
            }
        }
    }
}