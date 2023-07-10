using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.Structured.Kafka;

public class KafkaOutput : BufferedOutput
{
    private readonly KafkaLoggingOptions _options;
    private readonly IDisposable _producer;

    private readonly Headers _headers = new();

    public KafkaOutput(KafkaLoggingOptions options) : base(options.OutputOptions)
    {
        if (string.IsNullOrWhiteSpace(options.Topic))
            throw new ArgumentException("Must not empty", $"{nameof(options)}.{nameof(options.Topic)}");

        if (options.Serializer == null)
            throw new ArgumentException("Must not null", $"{nameof(options)}.{nameof(options.Serializer)}");

        _options = options;

        if (!string.IsNullOrWhiteSpace(_options.ContentType))
            _headers.Add("Content-Type", Encoding.UTF8.GetBytes(_options.ContentType!));

        if (_options.BatchSerializer == null)
        {
            var pb = new ProducerBuilder<byte[], IReadOnlyDictionary<string, object?>>(_options.ProducerConfig)
                .SetKeySerializer(Serializers.ByteArray)
                .SetValueSerializer(new ObjectSerializer<IReadOnlyDictionary<string, object?>>(_options.Serializer));

            if (_options.KafkaErrorHandler is { } handler) pb.SetErrorHandler((_, error) => handler(error));

            _producer = pb.Build();
        }
        else
        {
            var pb = new ProducerBuilder<byte[], IEnumerable<IReadOnlyDictionary<string, object?>>>(_options.ProducerConfig)
                .SetKeySerializer(Serializers.ByteArray)
                .SetValueSerializer(new ObjectSerializer<IEnumerable<IReadOnlyDictionary<string, object?>>>(_options.BatchSerializer));

            if (_options.KafkaErrorHandler is { } handler) pb.SetErrorHandler((_, error) => handler(error));

            _producer = pb.Build();
        }
    }

    // IProducer<byte[], IReadOnlyDictionary<string, object?>>
    protected override Task Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken) =>
        _producer is IProducer<byte[], IReadOnlyDictionary<string, object?>> producer
            ? Task.WhenAll(logs
                .Select(log => producer.ProduceAsync(_options.Topic, new Message<byte[], IReadOnlyDictionary<string, object?>>
                {
                    Headers = _headers,
                    Key = _options.CreateMessageKey(),
                    Timestamp = new Timestamp(log.Now),
                    Value = log.Data,
                }, cancellationToken)))
            : ((IProducer<byte[], IEnumerable<IReadOnlyDictionary<string, object?>>>)_producer)
            .ProduceAsync(_options.Topic, new Message<byte[], IEnumerable<IReadOnlyDictionary<string, object?>>>
            {
                Headers = _headers,
                Key = _options.CreateMessageKey(),
                Value = logs.Select(log => log.Data),
            }, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _producer.Dispose();
    }

    private class ObjectSerializer<T> : ISerializer<T>
    {
        private readonly Func<T, byte[]> _serializer;

        public ObjectSerializer(Func<T, byte[]> serializer) => _serializer = serializer;

        public byte[] Serialize(T logData, SerializationContext context) => _serializer(logData);
    }
}
