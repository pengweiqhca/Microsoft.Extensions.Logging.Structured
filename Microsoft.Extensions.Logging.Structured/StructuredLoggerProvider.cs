using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Extensions.Logging.Structured
{
    public abstract class StructuredLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private static readonly string ProxyIdentity = typeof(StructuredLogger).Namespace + ".Proxies";

        private static readonly ModuleBuilder Module = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(ProxyIdentity), AssemblyBuilderAccess.Run).DefineDynamicModule(ProxyIdentity);

        private static readonly IDictionary<string, Type> AliasTypeCache = new Dictionary<string, Type>();
        private static int _typeIndex;


        private readonly StructuredLoggingOptions _options;
        public IExternalScopeProvider? ScopeProvider { get; private set; }
        public IOutput Output => _options.Output;

        // ReSharper disable once PublicConstructorInAbstractClass
        public StructuredLoggerProvider(IOptionsSnapshot<StructuredLoggingOptions> options)
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

        internal static Type CreateSubclass(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));

            if (AliasTypeCache.TryGetValue(alias, out var type)) return type;

            lock (Module)
            {
                if (AliasTypeCache.TryGetValue(alias, out type)) return type;

                var typeBuilder = Module.DefineType(ProxyIdentity + "." + nameof(StructuredLoggerProvider) + _typeIndex++,
                    TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic);

                typeBuilder.SetParent(typeof(StructuredLoggerProvider));

                typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(ProviderAliasAttribute).GetConstructor(new[] { typeof(string) }), new object[] { alias }));

                var parameterTypes = new[] { typeof(IOptionsSnapshot<StructuredLoggingOptions>) };
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes)
                    .GetILGenerator();

                ctor.Emit(OpCodes.Ldarg_0);
                ctor.Emit(OpCodes.Ldarg_1);
                ctor.Emit(OpCodes.Call, typeof(StructuredLoggerProvider).GetConstructor(parameterTypes));
                ctor.Emit(OpCodes.Ret);

                return AliasTypeCache[alias] = typeBuilder.CreateTypeInfo().AsType();
            }
        }
    }
}
