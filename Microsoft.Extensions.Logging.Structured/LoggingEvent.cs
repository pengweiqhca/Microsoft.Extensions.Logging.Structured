using System;

namespace Microsoft.Extensions.Logging.Structured
{
    public readonly ref struct LoggingEvent
    {
        private readonly IExternalScopeProvider? _scopeProvider;

        public LoggingEvent(DateTimeOffset timeStamp, string categoryName, LogLevel logLevel,
            EventId eventId, object? message, string renderedMessage, Exception? exception, IExternalScopeProvider? scopeProvider)
        {
            CategoryName = categoryName;
            LogLevel = logLevel;
            EventId = eventId;
            Message = message;
            Exception = exception;
            RenderedMessage = renderedMessage;
            _scopeProvider = scopeProvider;
            TimeStamp = timeStamp;
        }

        public DateTimeOffset TimeStamp { get; }
        public string CategoryName { get; }
        public LogLevel LogLevel { get; }
        public EventId EventId { get; }
        public object? Message { get; }
        public Exception? Exception { get; }
        public string RenderedMessage { get; }

        public void ForEachScope(Action<object> callback) => _scopeProvider?.ForEachScope((obj, _) => callback(_), "");
    }
}
