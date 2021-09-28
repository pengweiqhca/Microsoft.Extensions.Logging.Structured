namespace Microsoft.Extensions.Logging.Structured
{
    public class BufferedOutputOptions
    {
        /// <summary>Which key can get datetime(offset) from log.</summary>
        public string DateTimeKey { get; set; } = "datetime";

        public int DueTime { get; set; } = 1000;

        public int Period { get; set; } = 1000;

        public int FlushTimeout { get; set; } = 5000;
    }
}
