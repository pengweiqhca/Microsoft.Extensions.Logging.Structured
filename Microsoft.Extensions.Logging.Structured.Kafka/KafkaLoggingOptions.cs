using Confluent.Kafka;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured.Kafka
{
    public class KafkaLoggingOptions : StructuredLoggingOptions
    {
        public string Topic { get; set; } = default!;

        public ProducerConfig ProducerConfig { get; } = new()
        {
            CompressionType = CompressionType.Gzip,
            SocketTimeoutMs = 5000,
            TopicMetadataRefreshIntervalMs = 60000,
            QueueBufferingMaxMessages = 10000,
            EnableDeliveryReports = false
        };

        public BufferedOutputOptions BufferedOutputOptions { get; } = new();

        /// <summary>Such as application/json;charset=UTF-8</summary>
        public string? ContentType { get; set; }

        /// <summary>Such as Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(logData))</summary>
        public Func<IReadOnlyDictionary<string, object?>, byte[]> Serializer { get; set; } = default!;

        public Action<Error>? KafkaErrorHandler { get; set; }
    }
}
