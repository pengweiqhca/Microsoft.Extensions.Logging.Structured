using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.Logging.Structured.Sls
{
    public static class LoggingBuilderExtensions
    {
        public static IStructuredLoggingBuilder<SlsLoggingOptions> AddSls(this ILoggingBuilder builder,
            Action<SlsLoggingOptions>? configureAction = null) =>
            builder.AddSls("sls", configureAction);

        public static IStructuredLoggingBuilder<SlsLoggingOptions> AddSls(this ILoggingBuilder builder, string name,
            Action<SlsLoggingOptions>? configureAction = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var httpClientName = $"SlsClient.{name}";

            var slb = builder.AddStructuredLog<SlsLoggingOptions>(name).SetOutput((options, provider) =>
                {
                    IHttpClientFactory? factory = null;

                    return new SlsOutput(() => (factory ??= provider.GetRequiredService<IHttpClientFactory>())
                            .CreateClient(httpClientName),
                        provider.GetRequiredService<RecyclableMemoryStreamManager>(),
                        options);
                });

            builder.Services.Configure<SlsLoggingOptions>(name, options =>
            {
                if (string.IsNullOrWhiteSpace(options.LogStore)) options.LogStore = name;
                if (string.IsNullOrWhiteSpace(options.Topic)) options.Topic = name;
            });

            builder.Services.AddHttpClient(httpClientName)
                .ConfigureHttpClient((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptionsMonitor<SlsLoggingOptions>>().Get(name);

                    client.BaseAddress = new Uri($"https://{options.Project}.{options.Region}");
                });

            if (configureAction != null) builder.Services.Configure(name, configureAction);

            return slb;
        }
    }
}
