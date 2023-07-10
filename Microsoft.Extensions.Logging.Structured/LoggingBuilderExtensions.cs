using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Structured;
using Microsoft.Extensions.Options;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging;

public static class LoggingBuilderExtensions
{
    public static IStructuredLoggingBuilder<TOptions> AddStructuredLog<TOptions>(this ILoggingBuilder builder, string alias, Action<TOptions>? configureAction = null)
        where TOptions : StructuredLoggingOptions, new()
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));

        builder.AddConfiguration();

        builder.Services.AddSingleton(typeof(ILoggerProvider), StructuredTypeHelper.CreateStructuredLoggerProviderSubclass<TOptions>(alias))
            .AddSingleton(typeof(IConfigureOptions<TOptions>), StructuredTypeHelper.CreateConfigureOptionsType<TOptions>(alias));

        if (configureAction != null) builder.Services.Configure(alias, configureAction);

        return new StructuredLoggingBuilder<TOptions>(builder, alias);
    }
}