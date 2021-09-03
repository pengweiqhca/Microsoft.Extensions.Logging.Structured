using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Logging.Structured
{
    public class StructuredLoggingBuilder<TOptions> : IStructuredLoggingBuilder<TOptions>
        where TOptions : StructuredLoggingOptions, new()
    {
        public StructuredLoggingBuilder(ILoggingBuilder builder, string @alias)
        {
            Alias = alias;

            Services = builder.Services;
        }

        public string Alias { get; }

        public IServiceCollection Services { get; }
    }
}
