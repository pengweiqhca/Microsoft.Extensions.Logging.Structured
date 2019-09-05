using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Tuhu.Extensions.Logging.Structured;
using Microsoft.Extensions.Options;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    public static class LoggingBuilderExtensions
    {
        public static StructuredLoggingBuilder<TOptions> AddStructuredLog<TOptions>(this ILoggingBuilder builder, string alias, Action<TOptions>? configureAction = null)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));

            if (configureAction != null) builder.Services.Configure(alias, configureAction);

            builder.AddConfiguration();

            builder.Services.AddTransient(typeof(ILoggerProvider), StructuredTypeHelper.CreateStructuredLoggerProviderSubclass<TOptions>(alias))
                .AddTransient(typeof(IConfigureOptions<TOptions>), StructuredTypeHelper.CreateConfigureOptionsType<TOptions>(alias));

            return new StructuredLoggingBuilder<TOptions>(builder, alias);
        }
    }
}
