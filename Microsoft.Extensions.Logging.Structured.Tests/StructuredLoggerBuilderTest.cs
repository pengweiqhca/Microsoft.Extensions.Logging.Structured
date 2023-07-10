using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests;

public class StructuredLoggerBuilderTest
{
    [Fact]
    public void Alias_Not_Allow_Null_Or_Empty()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddStructuredLog<StructuredLoggingOptions>(null!)));
        Assert.Throws<ArgumentNullException>(() => services.AddLogging(factory => factory.AddStructuredLog<StructuredLoggingOptions>("")));
    }

    [Fact]
    public void Output_Not_Allow_Null()
    {
        var services = new ServiceCollection();

        services.AddLogging(factory => factory.AddStructuredLog<StructuredLoggingOptions>("test").AddLayout("test", new DateTimeOffsetLayout()));

        using var provider = services.BuildServiceProvider(true);

        Assert.Throws<ArgumentNullException>(provider.GetRequiredService<ILoggerFactory>);
    }

    [Fact]
    public void Renderer_Not_Allow_Null()
    {
        var services = new ServiceCollection();

        services.AddLogging(factory => factory.AddStructuredLog<StructuredLoggingOptions>("test").SetOutput(new Mock<IOutput>().Object).AddLayout("test", new DateTimeOffsetLayout()));

        using var provider = services.BuildServiceProvider(true);

        try
        {
            provider.GetRequiredService<ILoggerFactory>();
        }
        catch (TargetInvocationException e)
        {
            Assert.IsType<ArgumentNullException>(e.InnerException);
        }
    }

    [Fact]
    public void Layouts_Not_Allow_Null_Or_Empty()
    {
        var services = new ServiceCollection();

        services.AddLogging(factory => factory.AddStructuredLog<StructuredLoggingOptions>("test").SetOutput(new Mock<IOutput>().Object));

        using var provider = services.BuildServiceProvider(true);

        Assert.Throws<ArgumentException>(provider.GetRequiredService<ILoggerFactory>);
    }

    [Fact]
    public void AddOutput()
    {
        var services = new ServiceCollection();

        services.AddLogging(lb => lb.AddStructuredLog<StructuredLoggingOptions>("test")
            .SetOutput(new Mock<IOutput>().Object)
            .AddLayout("abc", new DateTimeOffsetLayout()));

        using var provider = services.BuildServiceProvider(true);

        var loggerProvider = provider.GetRequiredService<ILoggerProvider>();
        Assert.IsAssignableFrom<StructuredLoggerProvider<StructuredLoggingOptions>>(loggerProvider);

        Assert.Equal("test", loggerProvider.GetType().GetCustomAttribute<ProviderAliasAttribute>()?.Alias);
    }
}