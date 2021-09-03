using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured
{
    public class StructuredLoggingOptions
    {
        private Dictionary<string, string> _layoutTypes = new();

        public Dictionary<string, Func<IServiceProvider, ILayout>> Layouts { get; } = new(StringComparer.OrdinalIgnoreCase);

        [Obsolete("Used by configuration")]
        public Dictionary<string, string> Layout
        {
            get => _layoutTypes;
            set
            {
                if (value == default!)
                {
                    _layoutTypes = new Dictionary<string, string>();

                    return;
                }

                _layoutTypes = value;

                foreach (var kv in value)
                {
                    if (Layouts.ContainsKey(kv.Key)) continue;

                    var type = Type.GetType(kv.Value);
                    if (type == null)
                        Layouts[kv.Key] = _ => new ConstLayout(kv.Value);
                    else
                        Layouts[kv.Key] = provider => (ILayout)ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
                }
            }
        }

        public IOutput Output { get; set; } = default!;

        public bool IgnoreNull { get; set; }

        public Action<Exception>? ExceptionHandler { get; set; }

        public Func<string, LogLevel, bool>? Filter { get; set; }

        public IStateRenderer StateRenderer { get; set; } = new DefaultStateRenderer();

        public StructuredLoggerOptions CreateLoggerOptions(IServiceProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            var options = new StructuredLoggerOptions
            {
                Output = Output,
                IgnoreNull = IgnoreNull,
                ExceptionHandler = ExceptionHandler,
                Filter = Filter,
                StateRenderer = StateRenderer
            };

            foreach (var kv in Layouts)
                options.Layouts[kv.Key] = kv.Value(provider);

            return options;
        }
    }
}
