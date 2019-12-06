using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Structured.Kafka;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests
{
    public class LoggingBuilderExtensionsTest
    {
        [Fact]
        public void AddStructuredLog()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Logging:Test:Layout:A", typeof(DateTimeLayout).FullName!}
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging(lb => lb.AddConfiguration(configuration.GetSection("Logging"))
                .AddStructuredLog<StructuredLoggingOptions>("Test", options => options.Layouts["A"] = new DateTimeOffsetLayout()));

            using var provider = services.BuildServiceProvider(true);

            var options = provider.GetRequiredService<IOptionsMonitor<StructuredLoggingOptions>>().Get("Test");

            Assert.Single(options.Layouts);
            Assert.True(options.Layouts.TryGetValue("A", out var layout));
            Assert.IsType<DateTimeOffsetLayout>(layout);
        }

        [Fact]
        public void AddKafka()
        {
            var services = new ServiceCollection();
            services.AddLogging(lb => lb.AddConfiguration(new ConfigurationBuilder().Build())
                .AddKafka(o =>
                {
                    o.Topic = "Abc";
                    o.ProducerConfig.EnableDeliveryReports = true;
                    o.Serializer = logData => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(logData));
                }));

            using var provider = services.BuildServiceProvider(true);

            var options = provider.GetRequiredService<IOptionsMonitor<KafkaLoggingOptions>>().Get(KafkaConstants.Kafka);

            Assert.Equal("Abc", options.Topic);
            Assert.True(options.ProducerConfig.EnableDeliveryReports);
        }
    }
}
