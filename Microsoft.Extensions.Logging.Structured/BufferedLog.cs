using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured
{
    public readonly struct BufferedLog
    {
        public BufferedLog(DateTimeOffset now, IReadOnlyDictionary<string, object?> data)
        {
            Now = now;
            Data = data;
        }

        public DateTimeOffset Now { get; }
        public IReadOnlyDictionary<string, object?> Data { get; }
    }
}
