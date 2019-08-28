using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Structured.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests
{
    public class KafkaOutputTest
    {
        [Fact]
        public void Not_Allow_Null()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddKafka(null, "a", "b")));
            Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddKafka("a", null, "b")));
            Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddKafka("a", "b", null)));
        }

        [Fact]
        public void Gzip_And_DisableDeliveryReports()
        {
            var services = new ServiceCollection();

            services.AddLogging(lb => lb.AddKafka());

            using var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptionsSnapshot<KafkaOutputOptions>>().Get(KafkaConstants.Kafka);

            Assert.NotNull(options.ProducerConfig);
            Assert.Equal(CompressionType.Gzip, options.ProducerConfig.CompressionType);
            Assert.False(options.ProducerConfig.EnableDeliveryReports);
        }

        [Fact]
        public void Output_Is_Kafka()
        {
            var services = new ServiceCollection();

            services.AddLogging(lb => lb.AddKafka("wcf.tuhu.work:19200", "test").AddLayout("test", new DateTimeOffsetLayout()));

            using var provider = services.BuildServiceProvider();


            var loggerProvider = provider.GetRequiredService<ILoggerProvider>();
            Assert.IsAssignableFrom<StructuredLoggerProvider>(loggerProvider);

            Assert.Equal(KafkaConstants.Kafka, loggerProvider.GetType().GetCustomAttribute<ProviderAliasAttribute>()?.Alias);

            Assert.IsType<KafkaOutput>(((StructuredLoggerProvider) loggerProvider).Output);
        }
    }
}
