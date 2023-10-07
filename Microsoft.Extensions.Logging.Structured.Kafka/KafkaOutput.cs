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
    private readonly IProducer<byte[], IReadOnlyDictionary<string, object?>> _producer;

    private readonly Headers _headers = new();

    public KafkaOutput(KafkaLoggingOptions options, Func<IReadOnlyDictionary<string, object?>, byte[]> serializer) : base(options.OutputOptions)
    {
        if (string.IsNullOrWhiteSpace(options.Topic))
            throw new ArgumentException("Must not be null or white space.", $"{nameof(options)}.{nameof(options.Topic)}");

        _options = options;

        if (!string.IsNullOrWhiteSpace(_options.ContentType))
            _headers.Add("Content-Type", Encoding.UTF8.GetBytes(_options.ContentType!));

        _producer = ObjectSerializer<IReadOnlyDictionary<string, object?>>.BuildProducer(_options, serializer);
    }

    // IProducer<byte[], IReadOnlyDictionary<string, object?>>
    protected override Task Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken) =>
        Task.WhenAll(logs
            .Select(log => _producer.ProduceAsync(_options.Topic, new Message<byte[], IReadOnlyDictionary<string, object?>>
            {
                Headers = _headers,
                Key = _options.CreateMessageKey(),
                Timestamp = new Timestamp(log.Now),
                Value = log.Data,
            }, cancellationToken)));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _producer.Flush();

        _producer.Dispose();
    }
}

public class KafkaBatchOutput : BufferedOutput
{
    private readonly KafkaLoggingOptions _options;
    private readonly IProducer<byte[], IEnumerable<IReadOnlyDictionary<string, object?>>> _producer;

    private readonly Headers _headers = new();

    public KafkaBatchOutput(KafkaLoggingOptions options, Func<IEnumerable<IReadOnlyDictionary<string, object?>>, byte[]> serializer) : base(options.OutputOptions)
    {
        if (string.IsNullOrWhiteSpace(options.Topic))
            throw new ArgumentException("Must not be null or white space.", $"{nameof(options)}.{nameof(options.Topic)}");

        _options = options;

        if (!string.IsNullOrWhiteSpace(_options.ContentType))
            _headers.Add("Content-Type", Encoding.UTF8.GetBytes(_options.ContentType!));

        _producer = ObjectSerializer<IEnumerable<IReadOnlyDictionary<string, object?>>>.BuildProducer(_options, serializer);
    }

    // IProducer<byte[], IReadOnlyDictionary<string, object?>>
    protected override Task Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken) =>
        _producer.ProduceAsync(_options.Topic, new Message<byte[], IEnumerable<IReadOnlyDictionary<string, object?>>>
        {
            Headers = _headers,
            Key = _options.CreateMessageKey(),
            Value = logs.Select(log => log.Data),
        }, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _producer.Flush();

        _producer.Dispose();
    }
}

file class ObjectSerializer<T> : ISerializer<T>
{
    private readonly Func<T, byte[]> _serializer;

    private ObjectSerializer(Func<T, byte[]> serializer) => _serializer = serializer;

    public byte[] Serialize(T logData, SerializationContext context) => _serializer(logData);

    public static IProducer<byte[], T> BuildProducer(KafkaLoggingOptions options, Func<T, byte[]> serializer)
    {
        var pb = new ProducerBuilder<byte[], T>(options.ProducerConfig)
            .SetKeySerializer(Serializers.ByteArray)
            .SetValueSerializer(new ObjectSerializer<T>(serializer));

        if (options.KafkaErrorHandler is { } errorHandler) pb.SetErrorHandler((_, error) => errorHandler(error));

        if (options.KafkaLogHandler is { } logHandler) pb.SetLogHandler((_, log) => logHandler(log));

        return pb.Build();
    }
}
