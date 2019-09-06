using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.Logging.Structured
{
    public interface IStructuredLoggingBuilder<out TOptions> where TOptions : StructuredLoggingOptions, new()
    {
        string Alias { get; }
        IServiceCollection Services { get; }
    }

    public class StructuredLoggingBuilder<TOptions> : IStructuredLoggingBuilder<TOptions> where TOptions : StructuredLoggingOptions, new()
    {
        public StructuredLoggingBuilder(ILoggingBuilder builder, string @alias)
        {
            Alias = alias;
            Services = builder.Services;
        }

        public string Alias { get; }
        public IServiceCollection Services { get; }
    }

    public static class StructuredLoggerBuilderExtensions
    {
        public static IStructuredLoggingBuilder<TOptions> SetOutput<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, Func<TOptions, IServiceProvider, IOutput> output)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (output == null) throw new ArgumentNullException(nameof(output));

            builder.Services.AddTransient<IConfigureOptions<TOptions>>(provider =>
                new ConfigureNamedOptions<TOptions>(builder.Alias, options =>
                    options.Output = output(options, provider)));

            return builder;
        }

        public static IStructuredLoggingBuilder<TOptions> SetOutput<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, IOutput output)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (output == null) throw new ArgumentNullException(nameof(output));

            builder.Services.Configure<TOptions>(builder.Alias, options => options.Output = output);

            return builder;
        }

        public static IStructuredLoggingBuilder<TOptions> SetStateRenderer<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, Func<TOptions, IServiceProvider, IStateRenderer> renderer)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));

            builder.Services.AddTransient<IConfigureOptions<TOptions>>(provider =>
                new ConfigureNamedOptions<TOptions>(builder.Alias, options =>
                    options.StateRenderer = renderer(options, provider)));

            return builder;
        }

        public static IStructuredLoggingBuilder<TOptions> SetStateRenderer<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, IStateRenderer renderer)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));

            builder.Services.Configure<TOptions>(builder.Alias, options => options.StateRenderer = renderer);

            return builder;
        }

        public static IStructuredLoggingBuilder<TOptions> AddLayout<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, string name, ILayout layout)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            builder.Services.Configure<TOptions>(builder.Alias, options => options.Layouts[name] = layout);

            return builder;
        }
    }
}
