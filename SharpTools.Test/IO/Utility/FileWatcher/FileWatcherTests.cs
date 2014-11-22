using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpTools.IO.Utility;
using SharpTools.Extend.Time;

namespace SharpTools.Test.IO.Utility
{
    [TestClass]
    public class FileWatcherTests
    {
        private const string ASSETS_DIR = @"Assets\FileWatcher";
        private const string SAMPLE_IMG = "ImageA.tga";
        private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private readonly FileWatcher _watcher;

        public FileWatcherTests()
        {
            _watcher = new FileWatcher();
        }

        [TestMethod]
        [DeploymentItem(ASSETS_DIR, ASSETS_DIR)]
        public async Task WaitFor_ReturnsWhenFileIsReady()
        {
            var imageName = "ImageB.tga";
            var source = Path.Combine(_baseDirectory, ASSETS_DIR, SAMPLE_IMG);
            var target = Path.Combine(_baseDirectory, ASSETS_DIR, imageName);

            var watcherTask = _watcher.WaitFor(target, 10.Seconds());
            var moveTask = Task.Run(() =>
            {
                using (var sourceStream = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var targetStream = File.Open(target, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    // Make the watcher wait a bit after the file is created
                    // to ensure it doesn't return before the file is actually ready
                    Thread.Sleep(5.Seconds());
                    sourceStream.CopyTo(targetStream);
                }
            });

            await watcherTask;

            // Validate that we can actually access the file when WaitFor returns
            Stream stream = null;
            try
            {
                stream = File.Open(target, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex)
            {
                Assert.Fail("Failed to open file. {0}. HResult: {1}", ex.Message, ex.HResult);
            }
            finally
            {
                if (stream != null)
                {
                    Assert.IsTrue(stream.CanRead);
                    Assert.IsTrue(stream.CanWrite);
                    stream.Close();
                }
            }

            // Make sure we let the move operation complete if it's still in progress
            await moveTask;
        }

        [TestMethod]
        [DeploymentItem(ASSETS_DIR, ASSETS_DIR)]
        [ExpectedException(typeof(TimeoutException))]
        public async Task WaitFor_ThrowsTimeoutExceptionWhenTimeoutExpires()
        {
            var imageName = "ImageC.tga";
            var source = Path.Combine(_baseDirectory, ASSETS_DIR, SAMPLE_IMG);
            var target = Path.Combine(_baseDirectory, ASSETS_DIR, imageName);

            var watcherTask = _watcher.WaitFor(target, 2.Seconds());
            var moveTask = Task.Run(() =>
            {
                using (var sourceStream = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var targetStream = File.Open(target, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    // Make the watcher wait a bit after the file is created
                    // to ensure it doesn't return before the file is actually ready
                    Thread.Sleep(5.Seconds());
                    sourceStream.CopyTo(targetStream);
                }
            });

            await watcherTask;
        }
    }
}
