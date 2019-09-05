using System;
using System.Collections.Generic;

namespace Tuhu.Extensions.Logging.Structured
{
    public interface IOutput : IDisposable
    {
        void Write(IReadOnlyDictionary<string, object?> logData);
    }
}
