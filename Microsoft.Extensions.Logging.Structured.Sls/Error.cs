namespace Microsoft.Extensions.Logging.Structured.Sls
{
    /// <summary>
    /// 错误信息
    /// </summary>
    public class Error
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        public override string ToString()
            => $"{ErrorCode}{(ErrorMessage == null ? string.Empty : $" ({ErrorMessage})")}";
    }
}
