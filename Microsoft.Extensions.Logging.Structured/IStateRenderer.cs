using System;
using System.Collections.Concurrent;

namespace Microsoft.Extensions.Logging.Structured
{
    public interface IStateRenderer
    {
        object? Render<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter);
    }

    public class DefaultStateRenderer : IStateRenderer
    {
        private readonly Func<Type, bool> _isFormattedType;
        private static readonly ConcurrentDictionary<Type, bool> StateCache = new ConcurrentDictionary<Type, bool>();

        public DefaultStateRenderer() : this(type => type.Assembly == typeof(ILogger<>).Assembly) { }

        public DefaultStateRenderer(Func<Type, bool> isFormattedType) => _isFormattedType = isFormattedType;

        public object? Render<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (state != null && (state is string || StateCache.GetOrAdd(state.GetType(), _isFormattedType)))
                return formatter(state, exception);

            return state;
        }
    }
}
