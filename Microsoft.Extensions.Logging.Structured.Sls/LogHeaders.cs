namespace Microsoft.Extensions.Logging.Structured.Sls;

/// <summary>
/// 日志服务扩展Header。
/// </summary>
public static class LogHeaders
{
    /// <summary>
    /// 请求的 Body 原始大小。
    /// 当无 Body 时，该字段为 0；
    /// 当 Body 是压缩数据，则为压缩前的原始数据大小。
    /// 该域取值范围为 0 ~ 3 * 1024 * 1024。
    /// 该字段为非必选字段，只在压缩时需要。
    /// </summary>
    public static readonly string BodyRawSize = "x-log-bodyrawsize";

    /// <summary>
    /// API 请求中 Body 部分使用的压缩方式。
    /// 如果不压缩可以不提供该请求头。
    /// </summary>
    public static readonly string CompressType = "x-log-compresstype";

    /// <summary>
    /// 签名计算方式。
    /// </summary>
    public static readonly string SignatureMethod = "x-log-signaturemethod";

    /// <summary>
    /// 使用 STS 临时身份发送数据。当使用 STS 临时身份时必填，其他情况不要填写。
    /// </summary>
    public static readonly string SecurityToken = "x-acs-security-token";

    /// <summary>
    /// 服务端产生的标示该请求的唯一 ID。该响应头与具体应用无关，主要用于跟踪和调查问题。
    /// 如果用户希望调查出现问题的 API 请求，可以向 Log Service 团队提供该 ID。
    /// </summary>
    public static readonly string RequestId = $"x-log-{nameof(RequestId).ToLower()}";
}
