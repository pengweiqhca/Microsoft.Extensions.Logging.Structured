using Moq;
using System;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests;
#nullable disable
public class LayoutTest
{
    [Fact]
    public void ConstLayout()
    {
        var key = Guid.NewGuid();

        Assert.Equal(key, new ConstLayout(key).Format(new LoggingEvent()));
    }

    [Fact]
    public void DateTimeLayout()
    {
        var now = DateTimeOffset.Now;

        Assert.Equal(now.LocalDateTime, new DateTimeLayout().Format(new LoggingEvent(now, null, LogLevel.Debug, new EventId(0), null, null, null, null)));
    }

    [Fact]
    public void DateTimeOffsetLayout()
    {
        var now = DateTimeOffset.Now;

        Assert.Equal(now, new DateTimeOffsetLayout().Format(new LoggingEvent(now, null, LogLevel.Debug, new EventId(0), null, null, null, null)));
    }

    [Fact]
    public void LoggerLayout()
    {
        var logger = Guid.NewGuid().ToString("N");

        Assert.Equal(logger, new CategoryNameLayout().Format(new LoggingEvent(DateTimeOffset.Now, logger, LogLevel.Debug, new EventId(0), null, null, null, null)));
    }

    [Fact]
    public void LogLevelLayout()
    {
        Assert.Equal(LogLevel.Debug.ToString(), new LogLevelLayout().Format(new LoggingEvent(DateTimeOffset.Now, null, LogLevel.Debug, new EventId(0), null, null, null, null)));
    }

    [Fact]
    public void EventIdLayout()
    {
        var eventId = new EventId(3, "Test");

        Assert.Equal(eventId.Name, new EventIdLayout().Format(new LoggingEvent(DateTimeOffset.Now, null, LogLevel.Debug, eventId, null, null, null, null)));
    }

    [Fact]
    public void MessageLayout()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(guid, new MessageLayout().Format(new LoggingEvent(DateTimeOffset.Now, null, LogLevel.Debug, default, guid, null, null, null)));

        Assert.Equal(guid.ToString("N"), new RenderedMessageLayout().Format(new LoggingEvent(DateTimeOffset.Now, null, LogLevel.Debug, default, guid, guid.ToString("N"), null, null)));
    }

    [Fact]
    public void ExceptionLayout()
    {
        Assert.Null(new ExceptionLayout().Format(new LoggingEvent(DateTimeOffset.Now, null, LogLevel.Debug, new EventId(0), null, null, null, null)));

        var ex = new Exception("abc");
        Assert.Equal(ex.ToString(), new ExceptionLayout().Format(new LoggingEvent(DateTimeOffset.Now, null, LogLevel.Debug, new EventId(0), null, null, ex, null)));

        var key = Guid.NewGuid().ToString("N");
        var moq = new Mock<IExceptionRenderer>();
        moq.Setup(o => o.Render(It.IsAny<Exception>())).Returns(key);

        Assert.Equal(key, new ExceptionLayout(moq.Object).Format(new LoggingEvent(DateTimeOffset.Now, null, LogLevel.Debug, new EventId(0), null, null, ex, null)));

        moq.Verify(o => o.Render(It.IsAny<Exception>()));
    }
}