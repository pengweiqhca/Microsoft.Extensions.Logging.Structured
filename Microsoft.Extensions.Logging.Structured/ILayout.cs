namespace Microsoft.Extensions.Logging.Structured;

public interface ILayout
{
    object? Format(LoggingEvent loggingEvent);
}

public abstract class PlainLayout : ILayout
{
    object? ILayout.Format(LoggingEvent loggingEvent) => Format(loggingEvent);

    public abstract string? Format(LoggingEvent loggingEvent);
}

public class ConstLayout : ILayout
{
    private readonly object _constObj;

    public ConstLayout(object constObj) => _constObj = constObj;

    public object Format(LoggingEvent loggingEvent) => _constObj;
}

public class DateTimeLayout : ILayout
{
    public object Format(LoggingEvent loggingEvent) => loggingEvent.TimeStamp.LocalDateTime;
}

public class DateTimeOffsetLayout : ILayout
{
    public object Format(LoggingEvent loggingEvent) => loggingEvent.TimeStamp;
}

public class CategoryNameLayout : PlainLayout
{
    public override string Format(LoggingEvent loggingEvent) => loggingEvent.CategoryName;
}

public class LogLevelLayout : PlainLayout
{
    public override string Format(LoggingEvent loggingEvent) => loggingEvent.LogLevel.ToString();
}

public class EventIdLayout : ILayout
{
    public object Format(LoggingEvent loggingEvent) => loggingEvent.EventId.ToString();
}

public class MessageLayout : ILayout
{
    public object? Format(LoggingEvent loggingEvent) => loggingEvent.Message;
}

public class RenderedMessageLayout : PlainLayout
{
    public override string Format(LoggingEvent loggingEvent) => loggingEvent.RenderedMessage;
}

public class ExceptionLayout : PlainLayout
{
    private readonly IExceptionRenderer? _renderer;

    public ExceptionLayout() { }
    public ExceptionLayout(IExceptionRenderer renderer) => _renderer = renderer;

    public override string? Format(LoggingEvent loggingEvent)
    {
        if (loggingEvent.Exception == null) return null;

        return _renderer == null ? loggingEvent.Exception.ToString() : _renderer.Render(loggingEvent.Exception);
    }
}