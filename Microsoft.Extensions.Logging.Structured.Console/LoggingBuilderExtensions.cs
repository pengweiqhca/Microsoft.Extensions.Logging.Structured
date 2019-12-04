using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace Microsoft.Extensions.Logging.Structured.Console
{
    public static class LoggingBuilderExtensions
    {
        public static IStructuredLoggingBuilder<ConsoleLoggingOptions> AddConsole(this ILoggingBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var slb = builder.AddStructuredLog<ConsoleLoggingOptions>("Console")
                .SetOutput((options, provider) => new ConsoleOutput(options));

            return slb;
        }

        public static IStructuredLoggingBuilder<ConsoleLoggingOptions> AddConsole(this ILoggingBuilder builder, Action<JsonSerializerSettings> configureAction)
        {
            if (configureAction == null) throw new ArgumentNullException(nameof(configureAction));

            builder.Services.ConfigureAll<ConsoleLoggingOptions>(options => configureAction(options.Settings ??= new JsonSerializerSettings()));

            return builder.AddConsole();
        }

        public static IStructuredLoggingBuilder<ConsoleLoggingOptions> AddConsole(this ILoggingBuilder builder, JsonSerializerSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            builder.Services.ConfigureAll<ConsoleLoggingOptions>(options => options.Settings = settings);

            return builder.AddConsole();
        }
    }
}
