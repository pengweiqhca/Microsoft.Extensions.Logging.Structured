namespace Microsoft.Extensions.Logging.Structured
{
    public class BufferedOutputOptions
    {
        public int DueTime { get; set; } = 1000;
        public int Period { get; set; } = 1000;
        public int Timeout { get; set; } = 5000;
    }
}
