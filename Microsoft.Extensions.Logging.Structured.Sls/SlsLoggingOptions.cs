using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.Structured.Sls
{
    public class SlsLoggingOptions : BufferedLoggingOptions<BufferedOutputOptions>
    {
        public string AccessKeyId { get; set; } = "";

        public string AccessKey { get; set; } = "";

        public string? StsToken { get; set; }

        public string Region { get; set; } = "";

        public string Project { get; set; } = "";

        public string LogStore { get; set; } = "";

        public string? Topic { get; set; }

        public string? Source { get; set; }

        public Func<object, IReadOnlyDictionary<string, string>> Serializer { get; set; } = default!;

        public Func<HttpContent, Task<Error?>> Deserializer { get; set; } = default!;
    }
}
