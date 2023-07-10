using Google.Protobuf;
using Ionic.Zlib;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.Structured.Sls;

public class LogClient
{
    private static readonly ProductInfoHeaderValue UserAgent = new("Mel.Sls", typeof(LogClient).Assembly.GetName().Version?.ToString() ?? "1.1.0");
    private readonly Func<HttpClient> _httpClient;
    private readonly Func<Credential> _credentialProvider;
    private readonly Func<HttpContent, Task<Error?>> _deserializer;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    public LogClient(Func<HttpClient> httpClient,
        Func<Credential> credentialProvider,
        Func<HttpContent, Task<Error?>> deserializer,
        RecyclableMemoryStreamManager memoryStreamManager)
    {
        _httpClient = httpClient;
        _credentialProvider = credentialProvider;
        _deserializer = deserializer;
        _memoryStreamManager = memoryStreamManager;
    }

    public async Task<HttpResponse> PutLogsAsync(string logStoreName, LogGroup logGroup, string? hashKey = null, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClient();
        if (httpClient.BaseAddress == null) throw new InvalidOperationException("Sls的入口域名不能为空");

        using var httpRequestMessage = BuildRequest(_credentialProvider(), logGroup,
            new Uri(httpClient.BaseAddress, string.IsNullOrEmpty(hashKey)
                ? $"/logstores/{logStoreName}/shards/lb"
                : $"/logstores/{logStoreName}/shards/route?key={hashKey}"));

        httpRequestMessage.Headers.Host = httpRequestMessage.RequestUri!.Host;

        //Suppress Activity.
        var activity = new Activity("Ignore");

        activity.ActivityTraceFlags |= ~ActivityTraceFlags.Recorded;
        activity.IsAllDataRequested = false;

        activity.Start();

        try
        {
            using var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

            return await ParseResponse(response).ConfigureAwait(false);
        }
        finally
        {
            activity.Stop();
        }
    }

    private HttpRequestMessage BuildRequest(Credential credential, IMessage message, Uri uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);

        request.Headers.Date = DateTimeOffset.Now;
        request.Headers.UserAgent.Add(UserAgent);
        request.Headers.TryAddWithoutValidation("x-log-apiversion", "0.6.0");

        if (!string.IsNullOrEmpty(credential.StsToken) &&
            (!request.Headers.TryGetValues(LogHeaders.SecurityToken, out var securityTokens) ||
             string.IsNullOrEmpty(securityTokens.FirstOrDefault())))
            request.Headers.Add(LogHeaders.SecurityToken, credential.StsToken);

        var encode = _memoryStreamManager.GetStream();
        var raw = _memoryStreamManager.GetStream();

        message.WriteTo(raw);

        raw.Position = 0;

        using (var cs = new ZlibStream(encode, CompressionMode.Compress, CompressionLevel.Default, true))
            raw.CopyTo(cs);

        request.Headers.TryAddWithoutValidation(LogHeaders.BodyRawSize, raw.Length.ToString());

        if (raw.Length <= encode.Length) Interlocked.Exchange(ref encode, raw).Dispose();
        else request.Headers.TryAddWithoutValidation(LogHeaders.CompressType, "deflate");

        encode.Position = 0;

        string md5;
        using (var hasher = MD5.Create())
            md5 = ByteArrayToHex(hasher.ComputeHash(encode));

        encode.Position = 0;

        request.Content = new StreamContent(encode)
        {
            Headers =
            {
                ContentLength = encode.Length,
                ContentType = new MediaTypeHeaderValue("application/x-protobuf"),
            }
        };

        request.Content.Headers.TryAddWithoutValidation("Content-MD5", md5);

        ComputeSignature(credential, request, uri.PathAndQuery);

        return request;
    }

    private static void ComputeSignature(Credential credential, HttpRequestMessage httpRequestMessage, string pathAndQuery)
    {
        httpRequestMessage.Headers.TryAddWithoutValidation(LogHeaders.SignatureMethod, "hmac-sha1");

        var list = new List<string>
        {
            httpRequestMessage.Method.Method,
            httpRequestMessage.Content != null && httpRequestMessage.Content.Headers
                .TryGetValues("Content-MD5", out var md5)
                ? md5.First()
                : "",
            httpRequestMessage.Content?.Headers.ContentType?.MediaType ?? "",
            (httpRequestMessage.Headers.Date ??= DateTimeOffset.UtcNow).ToString("r")
        };

        list.AddRange(httpRequestMessage.Headers
            .Where(x => x.Key.StartsWith("x-log", StringComparison.OrdinalIgnoreCase) || x.Key.StartsWith("x-acs", StringComparison.OrdinalIgnoreCase))
            .Select(x => new KeyValuePair<string, string>(x.Key.ToLower(), x.Value.SingleOrDefault(v => !string.IsNullOrEmpty(v)) /* Fault tolerance */))
            .Where(x => x.Value != null) // Remove empty header
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}:{x.Value}"));

        list.Add(pathAndQuery);

        using var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(credential.AccessKey));

        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("LOG", $"{credential.AccessKeyId}:{Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", list))))}");
    }

    private async Task<HttpResponse> ParseResponse(HttpResponseMessage httpResponseMessage)
    {
        string? requestId = null;

        if (httpResponseMessage.Headers.TryGetValues(LogHeaders.RequestId, out var requestIds))
            requestId = requestIds.FirstOrDefault(); // Fault tolerance.

        var readOnlyHeaders = ToNameValueCollection(httpResponseMessage);

        var error = httpResponseMessage.IsSuccessStatusCode ? null : await _deserializer(httpResponseMessage.Content).ConfigureAwait(false);

        return new HttpResponse(httpResponseMessage.StatusCode, requestId, readOnlyHeaders, error);
    }

    private static unsafe string ByteArrayToHex(IReadOnlyList<byte> buffer)
    {
        var str = new string('\0', buffer.Count * 2);

        fixed (char* c = str)
        {
            for (var i = 0; i < buffer.Count; ++i)
            {
                var b = (byte)(buffer[i] >> 4);

                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = (byte)(buffer[i] & 0xF);

                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
        }

        return str;
    }

    private static NameValueCollection ToNameValueCollection(HttpResponseMessage response)
    {
        var nv = new NameValueCollection();

        foreach (var header in response.Headers.Union(response.Content.Headers))
        foreach (var value in header.Value)
            nv.Add(header.Key, value);

        return nv;
    }
}
