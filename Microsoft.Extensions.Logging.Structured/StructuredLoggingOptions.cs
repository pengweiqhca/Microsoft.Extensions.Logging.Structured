using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured
{
    public class StructuredLoggingOptions
    {
        public Dictionary<string, ILayout> Layouts { get; } = new Dictionary<string, ILayout>(StringComparer.OrdinalIgnoreCase);

        public IOutput Output { get; set; } = default!;

        public IStateRenderer StateRenderer { get; set; } = new DefaultStateRenderer();
    }
}
