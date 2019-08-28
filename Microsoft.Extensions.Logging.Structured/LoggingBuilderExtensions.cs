using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Structured;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    public static class LoggingBuilderExtensions
    {
        public static StructuredLoggingBuilder AddStructuredLog(this ILoggingBuilder builder, string alias, Action<StructuredLoggingOptions>? configureAction = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));

            if (configureAction != null) builder.Services.Configure(alias, configureAction);

            builder.Services.AddTransient(typeof(ILoggerProvider), StructuredLoggerProvider.CreateSubclass(alias));

            return new StructuredLoggingBuilder(builder, alias);
        }
    }
}
