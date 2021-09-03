using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Structured.Console
{
    public static class LoggingBuilderExtensions
    {
        private static IStructuredLoggingBuilder<ConsoleLoggingOptions> AddConsole(this ILoggingBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var slb = builder.AddStructuredLog<ConsoleLoggingOptions>("Console")
                .SetOutput((options, provider) => new ConsoleOutput(options));

            return slb;
        }

        public static IStructuredLoggingBuilder<ConsoleLoggingOptions> AddConsole(this ILoggingBuilder builder, Action<ConsoleLoggingOptions> configureAction)
        {
            if (configureAction == null) throw new ArgumentNullException(nameof(configureAction));

            builder.Services.ConfigureAll(configureAction);

            return builder.AddConsole();
        }

        public static IStructuredLoggingBuilder<ConsoleLoggingOptions> AddConsole(this ILoggingBuilder builder, Func<IReadOnlyDictionary<string, object?>, string> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            builder.Services.ConfigureAll<ConsoleLoggingOptions>(options => options.Serializer = serializer);

            return builder.AddConsole();
        }
    }
}
