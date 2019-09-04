using Confluent.Kafka;
using Newtonsoft.Json;

namespace Microsoft.Extensions.Logging.Structured.Kafka
{
    public class KafkaLoggingOptions : StructuredLoggingOptions
    {
        public string Topic { get; set; } = default!;

        public ProducerConfig ProducerConfig { get; } = new ProducerConfig();

        public BufferedOutputOptions BufferedOutputOptions { get; } = new BufferedOutputOptions();

        public JsonSerializerSettings JsonSerializerOptions { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
