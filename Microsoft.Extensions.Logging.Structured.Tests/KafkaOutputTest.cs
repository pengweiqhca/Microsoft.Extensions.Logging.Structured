using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Structured.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests
{
    public class KafkaOutputTest
    {
        public ConfigurationBuilder builder { get; set; }
        public KafkaOutputTest()
        {
            builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");
        }

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
            var config = builder.Build().GetSection("Logging");
            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(config);
                lb.AddKafka() ;
            });
            using var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptionsSnapshot<KafkaLoggingOptions>>().Get(KafkaConstants.Kafka);

            Assert.NotNull(options.ProducerConfig);
            Assert.Equal(CompressionType.Gzip, options.ProducerConfig.CompressionType);
            Assert.False(options.ProducerConfig.EnableDeliveryReports);
        }

        [Fact]
        public void Output_Is_Kafka()
        {
            var services = new ServiceCollection();
            var config = builder.Build().GetSection("Logging");
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(config);
                lb.AddKafka()
                 .AddLayout("test", new DateTimeOffsetLayout());
            });

            using var provider = services.BuildServiceProvider();

            var loggerProvider = provider.GetRequiredService<ILoggerProvider>();
            Assert.IsAssignableFrom<StructuredLoggerProvider<KafkaLoggingOptions>>(loggerProvider);

            Assert.Equal(KafkaConstants.Kafka, loggerProvider.GetType().GetCustomAttribute<ProviderAliasAttribute>()?.Alias);

            Assert.IsType<KafkaOutput>(((StructuredLoggerProvider<KafkaLoggingOptions>) loggerProvider).Output);
        }

        [Fact]
        public void KafkaConfiguretion()
        {
            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    {$"Logging:{KafkaConstants.Kafka}:Topic", "abc"}
                }).Build().GetSection("Logging"));
                lb.AddKafka()
                    .AddLayout("test", new DateTimeOffsetLayout());
            });

            using var provider = services.BuildServiceProvider();

            Assert.Equal("abc", provider.GetRequiredService<IOptionsSnapshot<KafkaLoggingOptions>>().Get(KafkaConstants.Kafka).Topic);
        }

        [Fact]
        public void KafkaConfiguretionFromAppsettings()
        {
            var config = builder.Build().GetSection("Logging");
            var services = new ServiceCollection();
            services.AddLogging(lb =>
            {
                lb.AddConfiguration(config);
                lb.AddKafka()
                 .AddLayout("test", new DateTimeOffsetLayout());
            });

            using var provider = services.BuildServiceProvider();
            var ss = provider.GetRequiredService<IOptionsSnapshot<KafkaLoggingOptions>>().Get(KafkaConstants.Kafka);
            Assert.Equal(KafkaConstants.Kafka, ss.Topic);
            Assert.Equal("localhost:19092", ss.ProducerConfig.BootstrapServers);
        }
    }
}
