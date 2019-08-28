using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured
{
    public interface IOutput : IDisposable
    {
        void Write(IReadOnlyDictionary<string, object?> logData);
    }
}
