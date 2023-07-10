using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured;

public readonly ref struct LoggingEvent
{
    private readonly Lazy<IEnumerable<object?>> _scope;

    public LoggingEvent(DateTimeOffset timeStamp, string categoryName, LogLevel logLevel,
        EventId eventId, object? message, string renderedMessage, Exception? exception, IExternalScopeProvider? scopeProvider)
    {
        CategoryName = categoryName;
        LogLevel = logLevel;
        EventId = eventId;
        Message = message;
        Exception = exception;
        RenderedMessage = renderedMessage;
        TimeStamp = timeStamp;

        if (scopeProvider == null) _scope = new Lazy<IEnumerable<object?>>(Array.Empty<object?>);
        else
            _scope = new Lazy<IEnumerable<object?>>(() =>
            {
                var list = new List<object?>();

                scopeProvider.ForEachScope((obj, state) => state.Add(obj), list);

                return list.AsReadOnly();
            });
    }

    public DateTimeOffset TimeStamp { get; }
    public string CategoryName { get; }
    public LogLevel LogLevel { get; }
    public EventId EventId { get; }
    public object? Message { get; }
    public Exception? Exception { get; }
    public string RenderedMessage { get; }
    public IEnumerable<object?> Scope => _scope.Value;
}