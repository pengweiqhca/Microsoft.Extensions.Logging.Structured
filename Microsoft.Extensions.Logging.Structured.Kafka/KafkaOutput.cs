using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.Structured.Kafka
{
    public class KafkaOutput : BufferedOutput
    {
        private readonly KafkaLoggingOptions _options;
        private readonly IProducer<Guid, IReadOnlyDictionary<string, object?>> _producer;

        private readonly Headers _headers = new();

        public KafkaOutput(KafkaLoggingOptions options) : base(options.BufferedOutputOptions)
        {
            if (string.IsNullOrWhiteSpace(options.Topic))
                throw new ArgumentException("Must not empty", $"{nameof(options)}.{nameof(options.Topic)}");

            if (options.Serializer == null)
                throw new ArgumentException("Must not null", $"{nameof(options)}.{nameof(options.Serializer)}");

            _options = options;

            if (!string.IsNullOrWhiteSpace(_options.ContentType))
                _headers.Add("Content-Type", Encoding.UTF8.GetBytes(_options.ContentType!));

            var pb = new ProducerBuilder<Guid, IReadOnlyDictionary<string, object?>>(_options.ProducerConfig)
                .SetKeySerializer(new GuiSerializer())
                .SetValueSerializer(new ObjectSerializer(_options.Serializer));

            if (_options.KafkaErrorHandler != null)
            {
                var handler = _options.KafkaErrorHandler;

                pb.SetErrorHandler((_, error) => handler(error));
            }

            _producer = pb.Build();
        }

        protected override Task Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken) =>
            Task.WhenAll(logs
                .Select(log => _producer.ProduceAsync(_options.Topic, new Message<Guid, IReadOnlyDictionary<string, object?>>
                {
                    Headers = _headers,
                    Key = Guid.NewGuid(),
                    Timestamp = new Timestamp(log.Now),
                    Value = log.Data,
                }, cancellationToken)));

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _producer.Dispose();
        }

        private class GuiSerializer : ISerializer<Guid>
        {
            public byte[] Serialize(Guid data, SerializationContext context) => data.ToByteArray();
        }

        private class ObjectSerializer : ISerializer<IReadOnlyDictionary<string, object?>>
        {
            private readonly Func<IReadOnlyDictionary<string, object?>, byte[]> _serializer;

            public ObjectSerializer(Func<IReadOnlyDictionary<string, object?>, byte[]> serializer) => _serializer = serializer;

            public byte[] Serialize(IReadOnlyDictionary<string, object?> logData, SerializationContext context) => _serializer(logData);
        }
    }
}
