using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Extensions.Logging.Structured
{
    public abstract class BufferedOutput : IOutput
    {
        private readonly BufferedOutputOptions _options;
        private ConcurrentQueue<BufferedLog> _queue;
        private readonly Timer _timer;

        protected BufferedOutput(BufferedOutputOptions options)
        {
            _options = options;

            _queue = new ConcurrentQueue<BufferedLog>();
            _timer = new Timer(Send, null, _options.DueTime, _options.Period);
        }

        private void Send(object? state)
        {
            if (_queue.Count < 1) return;

            var queue = Interlocked.Exchange(ref _queue, new ConcurrentQueue<BufferedLog>());

            using var cts = new CancellationTokenSource(_options.Timeout);
            try
            {
                Write(Dequeue(queue), cts.Token);
            }
            catch
            {
                // ignored
            }
        }

        private static IEnumerable<BufferedLog> Dequeue(ConcurrentQueue<BufferedLog> queue)
        {
            while (queue.TryDequeue(out var log))
            {
                yield return log;
            }
        }

        public void Write(IReadOnlyDictionary<string, object?> logData) => _queue.Enqueue(new BufferedLog(DateTimeOffset.Now, logData));

        protected abstract void Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken);

        #region IDisposable Support
        private bool _disposed; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _timer.Dispose();

                Send(null);
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
