using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

                lb.AddStructuredLog<StructuredLoggingOptions>(key)
                    .SetOutput(moq.Object)
                    .AddLayout(key, new DateTimeOffsetLayout())
                    .AddLayout("msg", new MessageLayout());
            });

            using var provider = services.BuildServiceProvider(true);
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

                lb.AddStructuredLog<StructuredLoggingOptions>("test")
                    .SetOutput(moq.Object)
                    .AddLayout(key, new TestLayout());
            });

            using var provider = services.BuildServiceProvider(true);
            var logger = provider.GetRequiredService<ILogger<StructuredLoggerTest>>();
            provider.GetRequiredService<ILoggerFactory>().CreateLogger("test").BeginScope(provider);

            var ex = new Exception(key);
            logger.LogInformation(new EventId(3), ex, key);
            var loggingEvent = (LoggingEventWrapper?)list[0][key];

            Assert.NotNull(loggingEvent);
            Assert.True(loggingEvent!.TimeStamp < DateTimeOffset.Now);
            Assert.Equal(LogLevel.Information, loggingEvent.LogLevel);
            Assert.Equal(3, loggingEvent.EventId.Id);
            Assert.Equal(ex, loggingEvent.Exception);
            Assert.Equal(key, loggingEvent.Message);
            Assert.Equal(typeof(StructuredLoggerTest).FullName, loggingEvent.CategoryName);
            Assert.Equal(key, loggingEvent.RenderedMessage);
            Assert.Equal(provider, loggingEvent.Scope.FirstOrDefault());
        }

        [Fact]
        public void DateTimeKeyTest()
        {
            var list = new List<BufferedLog>();
            var key = Guid.NewGuid().ToString("N");
            var now = DateTimeOffset.Now;

            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    {$"Logging:{key}:LogLevel:Default", "Warning"}
                }).Build().GetSection("Logging"));

                lb.AddStructuredLog<StructuredLoggingOptions>(key)
                    .SetOutput(new TestOutput(new BufferedOutputOptions{DateTimeKey = key}, list))
                    .AddLayout(key, new ConstLayout(now))
                    .AddLayout("msg", new MessageLayout());
            });

            using (var provider = services.BuildServiceProvider(true))
            {
                var logger = provider.GetRequiredService<ILogger<StructuredLoggerTest>>();

                logger.LogWarning(key);
            }

            Assert.Single(list);

            Assert.Equal(now, list[0].Now);
            Assert.Equal(now, list[0].Data[key]);
        }

        [Fact]
        public void LayoutFromConfiguration()
        {
            var key = Guid.NewGuid().ToString("N");

            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    {$"Logging:{key}:Layout:{key}", typeof(DateTimeOffsetLayout).FullName!}
                }).Build().GetSection("Logging"));

                lb.AddStructuredLog<StructuredLoggingOptions>(key);
            });

            using var provider = services.BuildServiceProvider(true);

            Assert.NotNull(provider.GetRequiredService<IOptionsMonitor<StructuredLoggingOptions>>().Get(key).Layouts);
        }

        [Fact]
        public void FilterTest()
        {
            var key = Guid.NewGuid().ToString("N");

            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    {$"Logging:{key}:Layout:{key}", typeof(DateTimeOffsetLayout).FullName!}
                }).Build().GetSection("Logging"));

                lb.AddStructuredLog<StructuredLoggingOptions>(key, options => options.Filter = (_, level) => level == LogLevel.Trace).SetOutput(new Mock<IOutput>().Object);
            });

            services.Configure<LoggerFilterOptions>(options =>
            {
                options.Rules.Add(new LoggerFilterRule(null, null, null, (_, _, _) => true));
            });

            using var provider = services.BuildServiceProvider(true);

            Assert.NotNull(provider.GetRequiredService<IOptionsMonitor<StructuredLoggingOptions>>().Get(key).Filter);

            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(key);
            Assert.True(logger.IsEnabled(LogLevel.Trace));
            Assert.False(logger.IsEnabled(LogLevel.Error));
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
                Scope = loggingEvent.Scope;
            }

            public DateTimeOffset TimeStamp { get; }
            public string CategoryName { get; }
            public LogLevel LogLevel { get; }
            public EventId EventId { get; }
            public object? Message { get; }
            public Exception? Exception { get; }
            public string RenderedMessage { get; }
            public IEnumerable<object> Scope { get; }
        }

        private class TestOutput : BufferedOutput
        {
            private readonly IList<BufferedLog> _logs;

            public TestOutput(BufferedOutputOptions options, IList<BufferedLog> logs)
                : base(options) => _logs = logs;

            protected override Task Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken)
            {
                foreach (var log in logs) _logs.Add(log);

                return Task.CompletedTask;
            }
        }
    }
}
