using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.Structured
{
    public abstract class BufferedOutput : IOutput
    {
        private readonly BufferedOutputOptions _options;
        private ConcurrentQueue<BufferedLog>? _queue;
        private readonly Timer _timer;

        protected BufferedOutput(BufferedOutputOptions options)
        {
            _options = options;

            _queue = new ConcurrentQueue<BufferedLog>();
            _timer = new Timer(Send, null, _options.DueTime, _options.Period);
        }

        private async void Send(object? state)
        {
            if (_queue == null || _queue.Count < 1) return;

            var queue = Interlocked.Exchange(ref _queue, new ConcurrentQueue<BufferedLog>());

            using var cts = new CancellationTokenSource(_options.FlushTimeout);
            try
            {
                await Write(Dequeue(queue), cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private static IEnumerable<BufferedLog> Dequeue(ConcurrentQueue<BufferedLog> queue)
        {
            while (queue.TryDequeue(out var log))
            {
                yield return log;
            }
        }

        public void Write(IReadOnlyDictionary<string, object?> logData)
        {
            logData.TryGetValue(_options.DateTimeKey, out var time);

            _queue?.Enqueue(new BufferedLog(time switch
            {
#if NET6_0_OR_GREATER
                TimeOnly t => DateTimeOffset.Now.Date + t.ToTimeSpan(),
#endif
                DateTime dt => new DateTimeOffset(dt),
                DateTimeOffset dto => dto,
                _ => DateTimeOffset.Now,
            }, logData));
        }

        protected abstract Task Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken);

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

            _queue = null;

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
