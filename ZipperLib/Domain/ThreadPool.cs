using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ZipperLib.Domain
{
    public sealed class Pool : IDisposable
    {
        public int ThreadCount;
        public Pool()
        {
            _tasks = new ConcurrentQueue<Action>();
            _workers = new ConcurrentQueue<Thread>();
            _waiter = new AutoResetEvent(false);

            ThreadCount = Environment.ProcessorCount;
            for (int i = 0; i < ThreadCount; i++)
            {
                var worker = new Thread(Work)
                {
                    Name = $"Worker {i}",
                    Priority = ThreadPriority.Highest
                };
                worker.Start();
                _workers.Enqueue(worker);
            }
        }

        public void QueueTask(Action action)
        {
            if (_disallowAdd) { throw new InvalidOperationException("This Pool instance is in the process of being disposed, can't add anymore"); }
            if (_disposed) { throw new ObjectDisposedException("This Pool instance has already been disposed"); }
            _tasks.Enqueue(action);
            _waiter.Set();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                while (_tasks.Count > 0)
                {
                    _waiter.WaitOne();
                }

                _disposed = true;
                _disallowAdd = true;

                for (int i = 0; i < _workers.Count; i++)
                {
                    _waiter.Set();
                }
            }
        }

        public void Work()
        {
            while (true)
            {
                Action task;
                while (true)
                {
                    if (_disposed) return;
                    if (_tasks.TryDequeue(out task))
                    {
                        _workers.TryDequeue(out _);
                        _waiter.Set();
                        break;
                    }

                    _waiter.WaitOne();
                }

                task?.Invoke();
                _workers.Enqueue(Thread.CurrentThread);
            }
        }

        private readonly ConcurrentQueue<Thread> _workers; 
        private readonly ConcurrentQueue<Action> _tasks;
        private readonly EventWaitHandle _waiter;
        private bool _disallowAdd;
        private bool _disposed; 
    }
}
