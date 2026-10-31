using System.Threading;

namespace LyricsScraperNET.UnitTest.Threading
{
    /// <summary>
    /// Like a UI thread blocked in a sync call: posted continuations never run. Counts them.
    /// </summary>
    internal sealed class BlockedSynchronizationContext : SynchronizationContext
    {
        private int _postCount;

        public int PostCount => Volatile.Read(ref _postCount);

        public override void Post(SendOrPostCallback d, object state) => Interlocked.Increment(ref _postCount);

        public override void Send(SendOrPostCallback d, object state) => Interlocked.Increment(ref _postCount);
    }
}
