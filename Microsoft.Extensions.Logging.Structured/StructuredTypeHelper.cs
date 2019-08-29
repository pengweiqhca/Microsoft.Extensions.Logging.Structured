using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Extensions.Logging.Structured
{
    internal static class StructuredTypeHelper
    {
        private static readonly string ProxyIdentity = typeof(StructuredLogger).Namespace + ".Proxies";

        private static readonly ModuleBuilder Module = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(ProxyIdentity), AssemblyBuilderAccess.Run).DefineDynamicModule(ProxyIdentity);

        private static readonly IDictionary<string, Type> AliasTypeCache = new Dictionary<string, Type>();
        private static int _typeIndex;

        public static Type CreateStructuredLoggerProviderSubclass<TOptions>(string alias) where TOptions : StructuredLoggingOptions, new()
        {
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));

            if (AliasTypeCache.TryGetValue(alias, out var type)) return type;

            lock (Module)
            {
                if (AliasTypeCache.TryGetValue(alias, out type)) return type;

                var typeBuilder = Module.DefineType(ProxyIdentity + "." + nameof(StructuredLoggerProvider<TOptions>) + _typeIndex++,
                    TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic);

                typeBuilder.SetParent(typeof(StructuredLoggerProvider<TOptions>));

                typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(ProviderAliasAttribute).GetConstructor(new[] { typeof(string) }), new object[] { alias }));

                var parameterTypes = new[] { typeof(IOptionsSnapshot<TOptions>) };
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes)
                    .GetILGenerator();

                ctor.Emit(OpCodes.Ldarg_0);
                ctor.Emit(OpCodes.Ldarg_1);
                ctor.Emit(OpCodes.Call, typeof(StructuredLoggerProvider<TOptions>).GetConstructor(parameterTypes));
                ctor.Emit(OpCodes.Ret);

                return AliasTypeCache[alias] = typeBuilder.CreateTypeInfo().AsType();
            }
        }

        public static Type CreateConfigureOptionsType<TOptions>(string alias) where TOptions : StructuredLoggingOptions, new()
        {
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullException(nameof(alias));

            var cacheKey = $"{nameof(ILoggerProviderConfiguration<TOptions>)}.{alias}";

            if (AliasTypeCache.TryGetValue(cacheKey, out var type)) return type;

            lock (Module)
            {
                if (AliasTypeCache.TryGetValue(cacheKey, out type)) return type;

                var typeBuilder = Module.DefineType(ProxyIdentity + "." + nameof(ILoggerProviderConfiguration<TOptions>) + _typeIndex++,
                    TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic);

                typeBuilder.SetParent(typeof(NamedConfigureFromConfigurationOptions<TOptions>));

                var lpc = typeof(ILoggerProviderConfiguration<>).MakeGenericType(CreateStructuredLoggerProviderSubclass<TOptions>(alias));

                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { lpc })
                    .GetILGenerator();

                ctor.Emit(OpCodes.Ldarg_0);
                ctor.Emit(OpCodes.Ldstr, alias);
                ctor.Emit(OpCodes.Ldarg_1);
                // ReSharper disable once PossibleNullReferenceException
                ctor.Emit(OpCodes.Callvirt, lpc.GetProperty(nameof(ILoggerProviderConfiguration<TOptions>.Configuration)).GetGetMethod());
                ctor.Emit(OpCodes.Call, typeof(NamedConfigureFromConfigurationOptions<TOptions>).GetConstructor(new[] { typeof(string), typeof(IConfiguration) }));
                ctor.Emit(OpCodes.Ret);

                return AliasTypeCache[cacheKey] = typeBuilder.CreateTypeInfo().AsType();
            }
        }
    }
}
