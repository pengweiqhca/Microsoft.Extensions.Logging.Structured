using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Logging.Structured
{
    public static class StructuredLoggerBuilderExtensions
    {
        public static IStructuredLoggingBuilder<TOptions> SetOutput<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, Func<TOptions, IServiceProvider, IOutput> output)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (output == null) throw new ArgumentNullException(nameof(output));

            builder.Services.AddTransient<IPostConfigureOptions<TOptions>>(provider =>
                new PostConfigureOptions<TOptions>(builder.Alias, options =>
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

        public static IStructuredLoggingBuilder<TOptions> AddLayout<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, ILayout layout)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            return builder.AddLayout(GetName(layout.GetType()), _ => layout);
        }

        public static IStructuredLoggingBuilder<TOptions> AddLayout<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, params ILayout[] layouts)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (layouts == null || layouts.Length == 0) throw new ArgumentException(nameof(layouts));

            foreach (var layout in layouts)
            {
                var type = layout.GetType();

                var name = GetName(type);

                builder.Services.Configure<TOptions>(builder.Alias, options => options.Layouts[name] = _ => layout);
            }

            return builder;
        }

        public static IStructuredLoggingBuilder<TOptions> AddLayout<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, string name, ILayout layout)
            where TOptions : StructuredLoggingOptions, new() =>
            builder.AddLayout(name, _ => layout);

        public static IStructuredLoggingBuilder<TOptions> AddLayout<TOptions, TLayout>(this IStructuredLoggingBuilder<TOptions> builder)
            where TOptions : StructuredLoggingOptions, new()
            where TLayout : ILayout =>
            builder.AddLayout(GetName(typeof(TLayout)), provider => ActivatorUtilities.GetServiceOrCreateInstance<TLayout>(provider));

        public static IStructuredLoggingBuilder<TOptions> AddLayout<TOptions, TLayout>(this IStructuredLoggingBuilder<TOptions> builder, string name)
            where TOptions : StructuredLoggingOptions, new()
            where TLayout : ILayout =>
            builder.AddLayout(name, provider => ActivatorUtilities.GetServiceOrCreateInstance<TLayout>(provider));

        public static IStructuredLoggingBuilder<TOptions> AddLayout<TOptions>(this IStructuredLoggingBuilder<TOptions> builder, string name, Func<IServiceProvider, ILayout> layout)
            where TOptions : StructuredLoggingOptions, new()
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            builder.Services.Configure<TOptions>(builder.Alias, options => options.Layouts[name] = layout);

            return builder;
        }

        private static string GetName(MemberInfo type)
        {
            var name = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name != null) return name;

            name = type.Name.ToLower();

            if (name.EndsWith("Layout", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - 6);

            return name;
        }
    }
}
