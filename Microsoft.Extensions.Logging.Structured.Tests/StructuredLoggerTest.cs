using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests
{
    public class StructuredLoggerTest
    {
        [Fact]
        public void LoggerOutput()
        {
            var list = new List<IReadOnlyDictionary<string, object?>>();
            var key = Guid.NewGuid().ToString("N");

            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    {$"Logging:{key}:LogLevel:Default", "Warning"}
                }).Build().GetSection("Logging"));

                var moq = new Mock<IOutput>();
                moq.Setup(o => o.Write(It.IsAny<IReadOnlyDictionary<string, object?>>())).Callback<IReadOnlyDictionary<string, object?>>(log => list.Add(log));

                lb.AddStructuredLog(key)
                    .SetOutput(moq.Object)
                    .AddLayout(key, new DateTimeOffsetLayout());
            });

            using var provider = services.BuildServiceProvider();
            var logger = provider.GetRequiredService<ILogger<StructuredLoggerTest>>();

            logger.LogInformation(key);
            Assert.Empty(list);

            logger.LogWarning(key);
            Assert.Single(list);

            Assert.Contains(key, list[0].Keys);
            Assert.True((DateTimeOffset?)list[0][key] < DateTimeOffset.Now);
        }

        [Fact]
        public void LogEvent()
        {
            var list = new List<IReadOnlyDictionary<string, object?>>();
            var key = Guid.NewGuid().ToString("N");

            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                var moq = new Mock<IOutput>();
                moq.Setup(o => o.Write(It.IsAny<IReadOnlyDictionary<string, object?>>())).Callback<IReadOnlyDictionary<string, object?>>(log => list.Add(log));

                lb.AddStructuredLog("test")
                    .SetOutput(moq.Object)
                    .AddLayout(key, new TestLayout());
            });

            using var provider = services.BuildServiceProvider();
            var logger = provider.GetRequiredService<ILogger<StructuredLoggerTest>>();

            var ex = new Exception(key);
            logger.LogInformation(new EventId(3), ex, key);

            var loggingEvent = (LoggingEventWrapper?)list[0][key];

            Assert.NotNull(loggingEvent);
            Assert.True(loggingEvent.TimeStamp < DateTimeOffset.Now);
            Assert.Equal(LogLevel.Information, loggingEvent.LogLevel);
            Assert.Equal(3, loggingEvent.EventId.Id);
            Assert.Equal(ex, loggingEvent.Exception);
            Assert.Equal(key, loggingEvent.Message);
            Assert.Equal(typeof(StructuredLoggerTest).FullName, loggingEvent.CategoryName);
            Assert.Equal(key, loggingEvent.RenderedMessage);
        }

        private class TestLayout : ILayout
        {
            public object Format(LoggingEvent loggingEvent) => new LoggingEventWrapper(loggingEvent);
        }

        private class LoggingEventWrapper
        {
            public LoggingEventWrapper(LoggingEvent loggingEvent)
            {
                TimeStamp = loggingEvent.TimeStamp;
                CategoryName = loggingEvent.CategoryName;
                LogLevel = loggingEvent.LogLevel;
                EventId = loggingEvent.EventId;
                Message = loggingEvent.Message;
                Exception = loggingEvent.Exception;
                RenderedMessage = loggingEvent.RenderedMessage;
            }

            public DateTimeOffset TimeStamp { get; }
            public string CategoryName { get; }
            public LogLevel LogLevel { get; }
            public EventId EventId { get; }
            public object? Message { get; }
            public Exception? Exception { get; }
            public string RenderedMessage { get; }
        }
    }
}
