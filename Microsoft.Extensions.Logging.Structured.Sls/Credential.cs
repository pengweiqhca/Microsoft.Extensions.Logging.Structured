namespace Microsoft.Extensions.Logging.Structured.Sls
{
    /// <summary>
    /// 身份验证凭据。
    /// </summary>
    public class Credential
    {
        public string AccessKeyId { get; }

        public string AccessKey { get; }

        public string? StsToken { get; }

        public Credential(string accessKeyId, string accessKey, string? stsToken = null)
        {
            AccessKeyId = accessKeyId;
            AccessKey = accessKey;
            StsToken = stsToken;
        }
    }
}
