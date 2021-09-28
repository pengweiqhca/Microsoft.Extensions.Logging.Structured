namespace Microsoft.Extensions.Logging.Structured
{
    public class BufferedLoggingOptions<TOptions> : StructuredLoggingOptions
        where TOptions : BufferedOutputOptions, new()
    {
        public TOptions BufferedOutputOptions { get; } = new();
    }
}
