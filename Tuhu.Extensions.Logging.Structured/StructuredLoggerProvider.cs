using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

namespace Tuhu.Extensions.Logging.Structured
{
    public abstract class StructuredLoggerProvider<TOptions> : ILoggerProvider, ISupportExternalScope
        where TOptions : StructuredLoggingOptions, new()
    {

        private readonly TOptions _options;
        public IExternalScopeProvider? ScopeProvider { get; private set; }
        public IOutput Output => _options.Output;

        // ReSharper disable once PublicConstructorInAbstractClass
        public StructuredLoggerProvider(IOptionsSnapshot<TOptions> options)
        {
            var attr = GetType().GetCustomAttribute<ProviderAliasAttribute>();
            if (attr == null) throw new InvalidOperationException("Missing attribute ProviderAliasAttribute");

            _options = options.Get(attr.Alias);

            if (_options.Output == null) throw new ArgumentNullException(nameof(_options.Output));
            if (_options.StateRenderer == null) throw new ArgumentNullException(nameof(_options.StateRenderer));

            if (_options.Layouts == null) throw new ArgumentNullException(nameof(_options.Layouts));
            if (_options.Layouts.Count == 0) throw new ArgumentException("value is empty", nameof(_options.Layouts));
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName) => CreateLogger(categoryName);
        public StructuredLogger CreateLogger(string categoryName) => new StructuredLogger(categoryName, ScopeProvider, _options);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => ScopeProvider = scopeProvider;

        #region IDisposable Support
        private bool _disposed; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _options.Output.Dispose();
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
}
