namespace AcidJunkie.Analyzers.Logging;

internal static class DefaultLoggerLockProvider
{
    public const string MutexName = @"Global\AcidJunkie.Analyzers.Logging";

    public static IDisposable AcquireLock()
    {
        Mutex? mutex = null;
        try
        {
            mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
            if (!createdNew)
            {
                mutex.WaitOne();
            }

            return new LockReleaser(mutex);
        }
#pragma warning disable CA1031 // catch a more specific exception -> in our case, we need to catch everything. What we know is it can throw AbandonedMutexException for sure. But to be on the safe side, we catch all
        catch
#pragma warning restore CA1031
        {
            mutex?.Dispose();

            // when the process which owns the mutex terminates without releasing it, an exception is thrown.
            // therefore, we re-try until it works
            return AcquireLock();
        }
    }

    private sealed class LockReleaser : IDisposable
    {
        private Mutex? _mutex;

        public LockReleaser(Mutex mutex)
        {
            _mutex = mutex;
        }

        public void Dispose()
        {
            _mutex?.Dispose();
            _mutex = null;
        }
    }
}
