using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.Logging.Structured.Kafka;

public static class LoggingBuilderExtensions
{
    public static IStructuredLoggingBuilder<KafkaLoggingOptions> AddKafka(this ILoggingBuilder builder, string kafkaServer, string topic) => builder.AddKafka(KafkaConstants.Kafka, kafkaServer, topic);

    public static IStructuredLoggingBuilder<KafkaLoggingOptions> AddKafka(this ILoggingBuilder builder, string name, string kafkaServer, string topic)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(kafkaServer)) throw new ArgumentNullException(nameof(kafkaServer));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentNullException(nameof(topic));

        return builder.AddKafka(name, options =>
        {
            options.Topic = topic;
            options.ProducerConfig.BootstrapServers = kafkaServer;
        });
    }

    public static IStructuredLoggingBuilder<KafkaLoggingOptions> AddKafka(this ILoggingBuilder builder,
        Action<KafkaLoggingOptions>? configureAction = null) =>
        builder.AddKafka(KafkaConstants.Kafka, configureAction);

    public static IStructuredLoggingBuilder<KafkaLoggingOptions> AddKafka(this ILoggingBuilder builder, string name,
        Action<KafkaLoggingOptions>? configureAction = null)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

        var slb = builder.AddStructuredLog<KafkaLoggingOptions>(name)
            .SetOutput((options, _) =>
            {
                if (options.BatchSerializer != null) return new KafkaBatchOutput(options, options.BatchSerializer);

                if (options.Serializer != null) return new KafkaOutput(options, options.Serializer);

                throw new ArgumentException("Serializer or BatchSerializer cannot be both null.");
            });

        builder.Services.Configure<KafkaLoggingOptions>(name, options =>
        {
            options.ProducerConfig.CompressionType = CompressionType.Gzip;
            options.ProducerConfig.EnableDeliveryReports = false;
        });

        if (configureAction != null) builder.Services.Configure(name, configureAction);

        return slb;
    }
}
