using Newtonsoft.Json;

namespace Microsoft.Extensions.Logging.Structured.Console
{
    public class ConsoleLoggingOptions : StructuredLoggingOptions
    {
        public JsonSerializerSettings? Settings { get; set; }
    }
}
