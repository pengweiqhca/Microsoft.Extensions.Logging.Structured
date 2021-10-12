using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Extensions.Logging.Structured.Sls
{
    /// <summary>
    /// 服务响应包装对象，包含未反序列化的原始数据，可通过 <c>ReadXxxAsync()</c> 方法读取原始报文。
    /// </summary>
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; }

        public string? RequestId { get; }

        public NameValueCollection Headers { get; }

        public Error? Error { get; }

        public HttpResponse(HttpStatusCode statusCode, string? requestId, NameValueCollection headers, Error? error)
        {
            StatusCode = statusCode;
            RequestId = requestId;
            Headers = headers;
            Error = error;
        }

        public override string ToString() => $"[{RequestId}] {StatusCode}{(Error == null ? string.Empty : " Error:" + Error)}";
    }
}
