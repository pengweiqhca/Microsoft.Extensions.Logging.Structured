namespace Microsoft.Extensions.Logging.Structured
{
    public class BufferedOutputOptions
    {
        public int DueTime { get; set; } = 1000;
        public int Period { get; set; } = 1000;
        public int FlushTimeout { get; set; } = 5000;
    }
}
