using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Structured.Sls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.IO;
using Xunit;
using Error = Microsoft.Extensions.Logging.Structured.Sls.Error;

namespace Microsoft.Extensions.Logging.Structured.Tests;

public class SlsOutputTest
{
    public ConfigurationBuilder Builder { get; set; }
    public SlsOutputTest()
    {
        Builder = new ConfigurationBuilder();
        Builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
    }

    [Fact]
    public void Not_Allow_Null()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddSls(default!, null)));
    }

    [Fact]
    public void Output_Is_Sls()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new RecyclableMemoryStreamManager());

        var config = Builder.Build().GetSection("Logging");
        services.AddLogging(lb =>
        {
            lb.AddConfiguration(config);
            lb.AddSls(o =>
                {
                    o.Serializer = log =>
                    {
                        var json = (JObject)JToken.FromObject(log);

                        var dic = new Dictionary<string, string?>();

                        foreach (var kv in json)
                            dic[kv.Key] = kv.Value?.ToString();

                        return dic;
                    };
                    o.Deserializer = context => context.ReadFromJsonAsync<Error?>();
                })
                .AddLayout("test", new DateTimeOffsetLayout());
        });

        using var provider = services.BuildServiceProvider(true);

        var loggerProvider = provider.GetRequiredService<ILoggerProvider>();
        Assert.IsAssignableFrom<StructuredLoggerProvider<SlsLoggingOptions>>(loggerProvider);

        Assert.Equal("sls", loggerProvider.GetType().GetCustomAttribute<ProviderAliasAttribute>()?.Alias);

        Assert.IsType<SlsOutput>(((StructuredLoggerProvider<SlsLoggingOptions>)loggerProvider).Output);
    }
}