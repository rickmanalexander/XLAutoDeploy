using System;
using System.Threading;

namespace XLAutoDeploy.Updates
{
    /// <summary>
    /// A threadsafe timer that wraps a <see cref="System.Threading.Timer"/> and
    /// provides a mechanism for executing <see cref="Action"/>s 
    /// on a thread pool thread at a specified interval.
    /// </summary>  
    internal sealed class NonOverlappingTimer : IDisposable
    {
        private Timer _timer;
        private int _interval = 0;
        private Action _callBack;
        private readonly int _timeout = 0;
        private readonly object _threadLock = new object();
        private long _disposedCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonOverlappingTimer"/> class with 
        /// the interval (in terms of milliseconds) at which the 
        /// callback method is invoked, as well as an infinite timeout for the dispose method. 
        /// </summary>  
        /// <remarks>
        /// It is recommended that the callback method support cancellation to avoid the dispose
        /// method potentially waiting for an infinite amount of time. 
        /// </remarks> 
        public NonOverlappingTimer(int interval, Action callBack) : this(interval, callBack, Timeout.Infinite)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonOverlappingTimer"/> class with 
        /// the interval (in terms of milliseconds) at which the 
        /// callBack method is invoked, as well as the timeout (in terms of milliseconds) 
        /// that the dispose method will wait for the running callBack to complete. 
        /// </summary>  
        /// <remarks>
        /// If the timeout is exceeded then a <see cref="TimeoutException"/> will be 
        /// thrown. It is recommended that the callback method support cancellation to 
        /// avoid the dispose method potentially waiting for a period exceeding the 
        /// timeout. 
        /// </remarks>
        public NonOverlappingTimer(int interval, Action callBack, int timeout)
        {
            if (interval < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(interval));
            }

            _interval = interval;
            _callBack = callBack;
            _timeout = timeout;

            // This constructor specifies an infinite due time before the first callback
            // and an infinite interval between callbacks, preventing the timer from
            // firing until NonOverlappingTimer.Start() is called.
            _timer = new Timer(OnTimerElaspsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>  
        /// <remarks>
        /// The callback will not be invoked until the interval specified 
        /// in the constructor has elapsed. 
        /// </remarks>
        public void Start()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Reset();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>  
        /// <remarks>
        /// Since the callback method specified for a <see cref="System.Threading.Timer"/>  
        /// runs on the <see cref="ThreadPool"/>, it is possible for the timer to elapse
        /// after this method is called (i.e. a race condition). This is why the
        /// callback method specified should be reentrant.
        /// </remarks>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Resets the timer to run in now + interval specified in the constructor.
        /// </summary> 
        /// <remarks>
        /// This may not take effect immediately, because the callback method 
        /// could be running when this method is called. 
        /// </remarks>
        public void Reset()
        {
            _timer.Change(_interval, Timeout.Infinite);
        }

        /// <summary>
        /// Changes the timer to run in now + interval.
        /// </summary> 
        /// <remarks>
        /// The may not take effect imediately, because the callback method 
        /// could be running when it is called. 
        /// </remarks>
        public void Change(int interval)
        {
            if (interval < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(interval));
            }

            Interlocked.Exchange(ref _interval, interval);

            _timer.Change(interval, Timeout.Infinite);
        }

        /// <summary>
        /// Disposes of the underlying <see cref="Timer"/>. 
        /// </summary>  
        /// <remarks>
        /// Since the callback method specified for a <see cref="Timer"/>  
        /// runs on the <see cref="ThreadPool"/>, it is possible for the timer to elapse
        /// after this method is called (i.e. a race condition). This method
        /// waits for the queued callback to finish before disposing. If a callback method is a 
        /// long running process, it may take a while for 
        /// <see cref="System.Threading.Timer.Dispose(WaitHandle)"/> to signal, which is why
        /// it is recommended that the callback method to support cancellation.
        /// </remarks>
        // See: https://stackoverflow.com/a/15902261/9743237
        public void Dispose()
        {
            if (Interlocked.Read(ref _disposedCount) > 0)
                return;

            // Wait for timer queue to be emptied, before we continue.
            // Timer threads should have left the callback method given.
            // - http://woowaabob.blogspot.dk/2010/05/properly-disposing-systemthreadingtimer.html
            // - http://blogs.msdn.com/b/danielvl/archive/2011/02/18/disposing-system-threading-timer.aspx
            lock (_threadLock)
            {
                if (_timer == null)
                    return;

                Stop();

                // When Dispose(waitHandle) completes, it signals the specified waitHandle.
                // This overload of the Dispose method is used to block until it is
                // certain that the _timer has been disposed. The timer is not disposed
                // until all currently queued callbacks have completed.
                var waitHandle = new ManualResetEvent(false);
                if (_timer.Dispose(waitHandle))
                {
                    if (!waitHandle.WaitOne(_timeout))
                    {
                        throw new TimeoutException($"Timeout waiting for {nameof(NonOverlappingTimer)} to be disposed.");
                    }

                    waitHandle.Close();   // Only close if Dispose has completed succesfully

                    _timer = null;
                    _callBack = null;

                    Interlocked.Increment(ref _disposedCount);
                }
            }
        }

        // See: https://codereview.stackexchange.com/q/139966/188344
        // See: https://stackoverflow.com/a/15902261/9743237
        private void OnTimerElaspsed(object state)
        {
            if (Interlocked.Read(ref _disposedCount) > 0)
                return;

            // Ensure that we don't have multiple timers active at the same time.
            // Also prevents System.ObjectDisposedException when using the _timer
            // inside this method.
            if (Monitor.TryEnter(_threadLock))
            {
                try
                {
                    if (_timer == null)
                        return;

                    // Stop while callBack is running to prevent reentry if action takes
                    // a while.
                    Stop();

                    _callBack?.Invoke();

                    // Manually restart the timer to run in now + Interval.
                    Start();
                }
                finally
                {
                    Monitor.Exit(_threadLock);
                }
            }
        }
    }
}