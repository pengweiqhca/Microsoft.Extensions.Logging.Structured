using Microsoft.Extensions.Logging.Structured.Sls;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests
{
    public class LogClientTest
    {
        [Fact(Skip = "Need key")]
        public async Task PutLogsTest()
        {
            var client = new LogClient(() => new HttpClient
            {
                BaseAddress = new Uri("https://project.cn-shanghai.log.aliyuncs.com")
            },
                () => new Credential("accessKeyId", "accessKey"),
                context => context.ReadFromJsonAsync<Error?>(),
                new RecyclableMemoryStreamManager());

            var now = new DateTimeOffset(new DateTime(2020, 10, 10));
            var response = await client.PutLogsAsync("test", new LogGroup
            {
                Topic = "test",
                Source = "test-source",
                Logs =
                {
                    new List<Log>
                    {
                        new()
                        {
                            Time = (uint)now.ToUnixTimeSeconds(),
                            Contents = { new List<Log.Types.Content>
                            {
                                new() { Key = "appid", Value = "edf" },
                                new() { Key = "datetime", Value = now.ToString() },
                                new() { Key = "message", Value = new string('a', 1000) },
                            } }
                        }
                    }
                }
            }).ConfigureAwait(false);

            Assert.True(response.Error == null, response.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

}
