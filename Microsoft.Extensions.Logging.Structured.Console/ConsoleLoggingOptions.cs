using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured.Console
{
    public class ConsoleLoggingOptions : StructuredLoggingOptions
    {
        /// <summary>Such as JsonConvert.SerializeObject</summary>
        public Func<IReadOnlyDictionary<string, object?>, string>? Serializer { get; set; }
    }
}
