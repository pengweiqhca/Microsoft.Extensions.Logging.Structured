using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Structured;
using Microsoft.Extensions.Logging.Structured.Console;
using MySqlConnector;
using MySqlConnector.Logging;
using System.Text.Json;

namespace MySqlDemo;

internal class Program
{
    private static void Main()
    {
        using var provider = new ServiceCollection()
            .AddLogging(lb => lb.AddConsole(logData => JsonSerializer.Serialize(logData))
                .AddLayout(new DateTimeLayout(), new LogLevelLayout(), new RenderedMessageLayout(), new ExceptionLayout()))
            .BuildServiceProvider(true);

        MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(provider.GetRequiredService<ILoggerFactory>());

        using var connection = new MySqlConnection("server=localhost");

        connection.Open();
    }
}