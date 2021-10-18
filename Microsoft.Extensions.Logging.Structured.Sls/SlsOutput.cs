using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging.Structured.Sls
{
    public class SlsOutput : BufferedOutput
    {
        private readonly SlsLoggingOptions _options;
        private readonly LogClient _client;

        public SlsOutput(Func<HttpClient> httpClient,
            RecyclableMemoryStreamManager memoryStreamManager,
            SlsLoggingOptions options) : base(options.OutputOptions)
        {
            _options = options;

            if (string.IsNullOrEmpty(options.AccessKey))
                throw new ArgumentException("Must not empty", $"{nameof(options)}.{nameof(options.AccessKey)}");

            if (string.IsNullOrEmpty(options.AccessKeyId))
                throw new ArgumentException("Must not empty", $"{nameof(options)}.{nameof(options.AccessKeyId)}");

            if (string.IsNullOrEmpty(options.Region))
                throw new ArgumentException("Must not empty", $"{nameof(options)}.{nameof(options.Region)}");

            if (string.IsNullOrEmpty(options.Project))
                throw new ArgumentException("Must not empty", $"{nameof(options)}.{nameof(options.Project)}");

            if (string.IsNullOrEmpty(options.LogStore))
                throw new ArgumentException("Must not empty", $"{nameof(options)}.{nameof(options.LogStore)}");

            if (options.Serializer == default!)
                throw new ArgumentException("Must not null", $"{nameof(options)}.{nameof(options.Serializer)}");

            if (options.Deserializer == default!)
                throw new ArgumentException("Must not null", $"{nameof(options)}.{nameof(options.Deserializer)}");

            var credential = new Credential(
                options.AccessKeyId ?? throw new ArgumentNullException(nameof(options.AccessKeyId)),
                options.AccessKey ?? throw new ArgumentNullException(nameof(options.AccessKey)),
                options.StsToken);

            _client = new LogClient(httpClient, () => credential, options.Deserializer, memoryStreamManager);
        }

        protected override async Task Write(IEnumerable<BufferedLog> logs, CancellationToken cancellationToken)
        {
            var logGroup = new LogGroup
            {
                Topic = _options.Topic ?? "",
                Source = _options.Source ?? "",
                Logs =
                {
                    logs.Select(log => new Log
                    {
                        Time = (uint)log.Now.ToUnixTimeSeconds(),
                        Contents = { Convert(log.Data) }
                    })
                }
            };

            try
            {
                var response = await _client.PutLogsAsync(_options.LogStore, logGroup, null, cancellationToken).ConfigureAwait(false);
                if (response.Error != null)
                    Trace.TraceError($"{response.RequestId} {response.Error}");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            IEnumerable<Log.Types.Content> Convert(IReadOnlyDictionary<string, object?> log)
            {
                foreach (var kv in _options.Serializer(log))
                    if (!string.IsNullOrEmpty(kv.Value))
                        yield return new Log.Types.Content
                        {
                            Key = kv.Key,
                            Value = kv.Value
                        };
            }
        }
    }
}
