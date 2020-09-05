using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured
{
    public class StructuredLogger : ILogger, IDisposable
    {
        private readonly StructuredLoggingOptions _options;
        public string CategoryName { get; }
        public IExternalScopeProvider? ScopeProvider { get; }

        public StructuredLogger(string categoryName, IExternalScopeProvider? scopeProvider,
            StructuredLoggingOptions options)
        {
            _options = options;
            CategoryName = categoryName;
            ScopeProvider = scopeProvider;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var dictionary = new Dictionary<string, object?>();

            var message = _options.StateRenderer.Render(state, exception, formatter);

            var loggingEvent = new LoggingEvent(DateTimeOffset.Now, CategoryName, logLevel, eventId, message, formatter(state, exception), exception, ScopeProvider);

            try
            {
                foreach (var layout in _options.Layouts)
                {
					try
					{
						var value = layout.Value.Format(loggingEvent);

						if (!_options.IgnoreNull) dictionary[layout.Key] = value;
					}
					catch (Exception ex)
					{
						HandleException(ex);
					}
                }

                Log(dictionary);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        
        private void HandleException(Exception ex)
        {
			try
			{
				_options.ExceptionHandler?.Invoke(ex);
			}
			catch
			{
				// ignored
			}
        }

        public virtual void Log(IReadOnlyDictionary<string, object?> logData) => _options.Output.Write(logData);

        public bool IsEnabled(LogLevel logLevel)
        {
            var filer = _options.Filter;
            if (filer == null) return logLevel != LogLevel.None;

            return filer(CategoryName, logLevel);
        }

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? this;

        void IDisposable.Dispose() { }
    }
}
