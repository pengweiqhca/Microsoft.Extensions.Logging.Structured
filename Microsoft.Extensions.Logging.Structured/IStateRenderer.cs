using System;

namespace Microsoft.Extensions.Logging.Structured;

public interface IStateRenderer
{
    object? Render<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter);
}