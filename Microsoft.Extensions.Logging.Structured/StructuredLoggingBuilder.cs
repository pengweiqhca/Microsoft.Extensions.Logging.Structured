using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.Logging.Structured
{
    public interface IStructuredLoggingBuilder
    {
        string Alias { get; }
        IServiceCollection Services { get; }
    }

    public class StructuredLoggingBuilder : IStructuredLoggingBuilder
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
        public static IStructuredLoggingBuilder SetOutput(this IStructuredLoggingBuilder builder, Func<string, IServiceProvider, IOutput> output)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (output == null) throw new ArgumentNullException(nameof(output));

            builder.Services.AddTransient<IConfigureOptions<StructuredLoggingOptions>>(provider =>
                new ConfigureNamedOptions<StructuredLoggingOptions>(builder.Alias, options =>
                    options.Output = output(builder.Alias, provider)));

            return builder;
        }

        public static IStructuredLoggingBuilder SetOutput(this IStructuredLoggingBuilder builder, IOutput output)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (output == null) throw new ArgumentNullException(nameof(output));

            builder.Services.Configure<StructuredLoggingOptions>(builder.Alias, options => options.Output = output);

            return builder;
        }

        public static IStructuredLoggingBuilder SetStateRenderer(this IStructuredLoggingBuilder builder, Func<string, IServiceProvider, IStateRenderer> renderer)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));

            builder.Services.AddTransient<IConfigureOptions<StructuredLoggingOptions>>(provider =>
                new ConfigureNamedOptions<StructuredLoggingOptions>(builder.Alias, options =>
                    options.StateRenderer = renderer(builder.Alias, provider)));

            return builder;
        }

        public static IStructuredLoggingBuilder SetStateRenderer(this IStructuredLoggingBuilder builder, IStateRenderer renderer)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));

            builder.Services.Configure<StructuredLoggingOptions>(builder.Alias, options => options.StateRenderer = renderer);

            return builder;
        }

        public static IStructuredLoggingBuilder AddLayout(this IStructuredLoggingBuilder builder, string name, ILayout layout)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            builder.Services.Configure<StructuredLoggingOptions>(builder.Alias, options => options.Layouts[name] = layout);

            return builder;
        }
    }
}
