using Microsoft.Extensions.Logging.Structured.Sls;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.Logging.Structured.Tests;

public class LogClientTest
{
    [Fact]
    public async Task PutLogsTest()
    {
        var client = new LogClient(() => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:34065/")
            },
            () => new Credential("accessKeyId", "accessKey"),
            context => context.ReadFromJsonAsync<Error?>(),
            new RecyclableMemoryStreamManager());

        var now = new DateTimeOffset(new DateTime(2020, 10, 10));
        var response = await client.PutLogsAsync("test", new LogGroup
        {
            Topic = "log-internal-service-reqeust",
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
                            new() { Key = "response_milliseconds_taken", Value = "1.2" },
                            new() { Key = "request_begin_time", Value = now.ToString("o") },
                            new() { Key = "message", Value = new string('a', 1000) },
                        } }
                    },
                    new()
                    {
                        Time = (uint)now.ToUnixTimeSeconds(),
                        Contents = { new List<Log.Types.Content>
                        {
                            new() { Key = "appid", Value = "ccccc" },
                            new() { Key = "response_milliseconds_taken", Value = "1.2" },
                            new() { Key = "request_begin_time", Value = DateTimeOffset.Now.ToString("o") },
                            new() { Key = "message", Value = new string('b', 1000) },
                        } }
                    }
                }
            }
        }).ConfigureAwait(false);

        Assert.True(response.Error == null, response.ToString());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
