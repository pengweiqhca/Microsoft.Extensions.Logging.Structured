using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured;

public class StructuredLoggerOptions
{
    public Dictionary<string, ILayout> Layouts { get; } = new(StringComparer.OrdinalIgnoreCase);

    public IOutput Output { get; set; } = default!;

    public bool IgnoreNull { get; set; }

    public Action<Exception>? ExceptionHandler { get; set; }

    public Func<string, LogLevel, bool>? Filter { get; set; }

    public IStateRenderer StateRenderer { get; set; } = new DefaultStateRenderer();
}