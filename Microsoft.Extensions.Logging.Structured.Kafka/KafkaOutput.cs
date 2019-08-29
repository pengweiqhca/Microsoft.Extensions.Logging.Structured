using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Microsoft.Extensions.Logging.Structured.Kafka
{
    public class KafkaOutput : BufferedOutput
    {
        private readonly KafkaLoggingOptions _options;
        private readonly IProducer<string, object> _producer;

        private static readonly Headers Headers = new Headers
        {
            {"Content-Type", Encoding.UTF8.GetBytes("application/json;charset=UTF-8")}
        };

        public KafkaOutput(KafkaLoggingOptions options) : base(options.BufferedOutputOptions)
        {
            _options = options;
            _producer = new ProducerBuilder<string, object>(_options.ProducerConfig)
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(new ObjectSerializer(_options.JsonSerializerOptions))
                .SetErrorHandler((_, error) => Trace.TraceError(JsonSerializer.Serialize(error)))
                .Build();
        }

        protected override void Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken)
        {
            foreach (var log in logs)
                _producer.ProduceAsync(_options.Topic, new Message<string, object>
                {
                    Headers = Headers,
                    Key = Guid.NewGuid().ToString("N"),
                    Timestamp = new Timestamp(log.Now),
                    Value = log.Data,
                });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _producer.Dispose();
        }

        private class ObjectSerializer : ISerializer<object>
        {
            private readonly JsonSerializerOptions _options;

            public ObjectSerializer(JsonSerializerOptions options) => _options = options;

            public byte[] Serialize(object data, SerializationContext context) =>
                JsonSerializer.SerializeToUtf8Bytes(data, _options);
        }
    }
}
