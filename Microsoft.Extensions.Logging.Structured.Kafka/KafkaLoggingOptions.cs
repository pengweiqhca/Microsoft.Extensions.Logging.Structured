using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging.Structured.Kafka
{
    public class KafkaLoggingOptions : BufferedLoggingOptions<BufferedOutputOptions>
    {
        private Func<byte[]> _createMessageKey = () => Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N"));

        public string Topic { get; set; } = default!;

        public ProducerConfig ProducerConfig { get; } = new()
        {
            CompressionType = CompressionType.Gzip,
            SocketTimeoutMs = 5000,
            TopicMetadataRefreshIntervalMs = 60000,
            QueueBufferingMaxMessages = 10000,
            EnableDeliveryReports = false
        };

        /// <summary>Such as application/json;charset=UTF-8</summary>
        public string? ContentType { get; set; }

        /// <summary>Such as Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(logData))</summary>
        public Func<IReadOnlyDictionary<string, object?>, byte[]> Serializer { get; set; } = default!;

        public Action<Error>? KafkaErrorHandler { get; set; }

        public Func<byte[]> CreateMessageKey
        {
            get => _createMessageKey;
            set => _createMessageKey = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
