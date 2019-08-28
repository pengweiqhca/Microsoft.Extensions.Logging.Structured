using Confluent.Kafka;
using System.Text.Json;

namespace Microsoft.Extensions.Logging.Structured.Kafka
{
    public class KafkaOutputOptions
    {
        public string Topic { get; set; } = default!;

        public ProducerConfig ProducerConfig { get; } = new ProducerConfig();

        public BufferedOutputOptions BufferedOutputOptions { get; } = new BufferedOutputOptions();

        public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };
    }
}
