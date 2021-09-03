using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Logging.Structured
{
    public interface IStructuredLoggingBuilder<out TOptions>
        where TOptions : StructuredLoggingOptions, new()
    {
        string Alias { get; }

        IServiceCollection Services { get; }
    }
}
