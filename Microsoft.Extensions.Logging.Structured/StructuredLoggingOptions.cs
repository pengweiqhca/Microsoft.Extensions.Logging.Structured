using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured
{
    public class StructuredLoggingOptions
    {
        private Dictionary<string, string> _layoutTypes = new Dictionary<string, string>();
        public Dictionary<string, ILayout> Layouts { get; } = new Dictionary<string, ILayout>(StringComparer.OrdinalIgnoreCase);

        public ILogProcessor Processor { get; set; }

        [Obsolete("Used by configuration")]
        public Dictionary<string, string> Layout
        {
            get => _layoutTypes;
            set
            {
                _layoutTypes = value;

                if (value == null) return;

                foreach (var kv in value)
                {
                    if (Layouts.ContainsKey(kv.Key)) continue;

                    var type = Type.GetType(kv.Value);
                    if (type == null)
                        Layouts[kv.Key] = new ConstLayout(kv.Value);
                    else
                        Layouts[kv.Key] = (ILayout)Activator.CreateInstance(type);
                }
            }
        }

        public IOutput Output { get; set; } = default!;

        public IStateRenderer StateRenderer { get; set; } = new DefaultStateRenderer();
    }
    public interface ILogProcessor
    {
        IDictionary<string, object> Process(IDictionary<string, object> log);
    }
}
