using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured.Console;

public class ConsoleOutput : IOutput
{
    private readonly Func<IReadOnlyDictionary<string, object?>, string> _serializer;

    public ConsoleOutput(ConsoleLoggingOptions options) => _serializer = options.Serializer ?? throw new ArgumentNullException(nameof(options.Serializer));

    public void Dispose() { }

    public void Write(IReadOnlyDictionary<string, object?> logData) => System.Console.WriteLine(_serializer(logData));
}