using System;
using System.Collections.Concurrent;

namespace Microsoft.Extensions.Logging.Structured;

public class DefaultStateRenderer : IStateRenderer
{
    private readonly Func<Type, bool> _isFormattedType;
    private static readonly ConcurrentDictionary<Type, bool> StateCache = new();

    public DefaultStateRenderer() : this(type => type.Assembly == typeof(ILogger<>).Assembly) { }

    public DefaultStateRenderer(Func<Type, bool> isFormattedType) => _isFormattedType = isFormattedType;

    public object? Render<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var type = formatter.Method.DeclaringType ?? formatter.Method.ReflectedType;

        if (type != null && _isFormattedType(type) &&
            (state == null || state is not string && !StateCache.GetOrAdd(state.GetType(), _isFormattedType)))
            return state;

        return formatter(state, exception);
    }
}