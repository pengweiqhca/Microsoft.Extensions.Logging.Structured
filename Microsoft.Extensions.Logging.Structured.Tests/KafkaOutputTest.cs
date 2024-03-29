﻿using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Structured.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests;

public class KafkaOutputTest
{
    public ConfigurationBuilder Builder { get; set; }
    public KafkaOutputTest()
    {
        Builder = new ConfigurationBuilder();
        Builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
    }

    [Fact]
    public void Not_Allow_Null()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddKafka(default!, "a", "b")));
        Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddKafka("a", default!, "b")));
        Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddKafka("a", "b", default!)));
    }

    [Fact]
    public void Gzip_And_DisableDeliveryReports()
    {
        var config = Builder.Build().GetSection("Logging");
        var services = new ServiceCollection();
        services.AddLogging(lb =>
        {
            lb.AddConfiguration(config);
            lb.AddKafka(o => o.Serializer = logData => JsonSerializer.SerializeToUtf8Bytes(logData))
                .AddLayout("level", new LogLevelLayout()).AddLayout("msg", new RenderedMessageLayout());
        });
        using var provider = services.BuildServiceProvider(true);
        var options = provider.GetRequiredService<IOptionsMonitor<KafkaLoggingOptions>>().Get(KafkaConstants.Kafka);
        ILogger<KafkaOutputTest> logger = provider.GetRequiredService<ILogger<KafkaOutputTest>>();
        logger.LogInformation("testqqqqpwd");
        logger.LogInformation("idcardqqqq");
        Assert.NotNull(options.ProducerConfig);
        Assert.Equal(CompressionType.Gzip, options.ProducerConfig.CompressionType);
        Assert.False(options.ProducerConfig.EnableDeliveryReports);
    }

    [Fact]
    public void Output_Is_Kafka()
    {
        var services = new ServiceCollection();
        var config = Builder.Build().GetSection("Logging");
        services.AddLogging(lb =>
        {
            lb.AddConfiguration(config);
            lb.AddKafka(o => o.Serializer = logData => JsonSerializer.SerializeToUtf8Bytes(logData))
                .AddLayout("test", new DateTimeOffsetLayout());
        });

        using var provider = services.BuildServiceProvider(true);

        var loggerProvider = provider.GetRequiredService<ILoggerProvider>();
        Assert.IsAssignableFrom<StructuredLoggerProvider<KafkaLoggingOptions>>(loggerProvider);

        Assert.Equal(KafkaConstants.Kafka, loggerProvider.GetType().GetCustomAttribute<ProviderAliasAttribute>()?.Alias);

        Assert.IsType<KafkaOutput>(((StructuredLoggerProvider<KafkaLoggingOptions>)loggerProvider).Output);
    }

    [Fact]
    public void KafkaConfiguration()
    {
        var services = new ServiceCollection();
        services.AddLogging(lb =>
        {
            lb.AddConfiguration(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                {$"Logging:{KafkaConstants.Kafka}:Topic", "abc"}
            }).Build().GetSection("Logging"));
            lb.AddKafka(o => o.Serializer = logData => JsonSerializer.SerializeToUtf8Bytes(logData))
                .AddLayout("test", new DateTimeOffsetLayout());
        });

        using var provider = services.BuildServiceProvider(true);

        Assert.Equal("abc", provider.GetRequiredService<IOptionsMonitor<KafkaLoggingOptions>>().Get(KafkaConstants.Kafka).Topic);
    }

    [Fact]
    public void KafkaConfigurationFromAppSettings()
    {
        var config = Builder.Build().GetSection("Logging");
        var services = new ServiceCollection();
        services.AddLogging(lb =>
        {
            lb.AddConfiguration(config);
            lb.AddKafka(o => o.Serializer = logData => JsonSerializer.SerializeToUtf8Bytes(logData))
                .AddLayout("test", new DateTimeOffsetLayout());
        });

        using var provider = services.BuildServiceProvider(true);
        var ss = provider.GetRequiredService<IOptionsMonitor<KafkaLoggingOptions>>().Get(KafkaConstants.Kafka);
        Assert.Equal(KafkaConstants.Kafka, ss.Topic);
        Assert.Equal("localhost:19092", ss.ProducerConfig.BootstrapServers);
    }
}