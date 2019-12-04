using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured.Console
{
    public class ConsoleOutput : IOutput
    {
        private readonly ConsoleLoggingOptions _options;

        public ConsoleOutput(ConsoleLoggingOptions options) => _options = options;

        public void Dispose() { }

        public void Write(IReadOnlyDictionary<string, object?> logData) => System.Console.WriteLine(JsonConvert.SerializeObject(logData, _options.Settings));
    }
}
