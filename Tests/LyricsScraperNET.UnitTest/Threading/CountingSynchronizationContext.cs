using System.Threading;

namespace LyricsScraperNET.UnitTest.Threading
{
    /// <summary>
    /// Runs posted continuations on the thread pool and counts them.
    /// </summary>
    internal sealed class CountingSynchronizationContext : SynchronizationContext
    {
        private int _postCount;

        public int PostCount => Volatile.Read(ref _postCount);

        public override void Post(SendOrPostCallback d, object state)
        {
            Interlocked.Increment(ref _postCount);
            base.Post(d, state);
        }
    }
}
