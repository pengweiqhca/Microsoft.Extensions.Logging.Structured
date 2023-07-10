using Microsoft.Extensions.Options;
using System;
using System.Reflection;

namespace Microsoft.Extensions.Logging.Structured;

public abstract class StructuredLoggerProvider<TOptions> : ILoggerProvider, ISupportExternalScope
    where TOptions : StructuredLoggingOptions, new()
{
    private readonly Lazy<StructuredLoggerOptions> _options;
    public IExternalScopeProvider? ScopeProvider { get; private set; }
    public IOutput Output => _options.Value.Output;

    // ReSharper disable once PublicConstructorInAbstractClass
    public StructuredLoggerProvider(IOptionsMonitor<TOptions> optionsMonitor, IServiceProvider provider)
    {
        var attr = GetType().GetCustomAttribute<ProviderAliasAttribute>();
        if (attr == null) throw new InvalidOperationException("Missing attribute ProviderAliasAttribute");

        var options = optionsMonitor.Get(attr.Alias);

        if (options.Output == null) throw new ArgumentNullException(nameof(options.Output));
        if (options.StateRenderer == null) throw new ArgumentNullException(nameof(options.StateRenderer));

        if (options.Layouts == null) throw new ArgumentNullException(nameof(options.Layouts));
        if (options.Layouts.Count == 0) throw new ArgumentException("value is empty", nameof(options.Layouts));

        _options = new Lazy<StructuredLoggerOptions>(() => options.CreateLoggerOptions(provider));
    }

    ILogger ILoggerProvider.CreateLogger(string categoryName) => CreateLogger(categoryName);
    public StructuredLogger CreateLogger(string categoryName) => new StructuredLogger(categoryName, ScopeProvider, _options.Value);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => ScopeProvider = scopeProvider;

        #region IDisposable Support
    private bool _disposed; // 要检测冗余调用

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _options.IsValueCreated)
            {
                _options.Value.Output.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }
        #endregion
}