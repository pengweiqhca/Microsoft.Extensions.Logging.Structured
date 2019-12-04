﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Structured;
using Microsoft.Extensions.Logging.Structured.Console;
using MySql.Data.MySqlClient;
using MySqlConnector.Logging;

namespace MySqlDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using var provider = new ServiceCollection()
                .AddLogging(lb => lb.AddConsole(logData => JsonSerializer.Serialize(logData))
                    .AddLayout(new DateTimeLayout(), new LogLevelLayout(), new RenderedMessageLayout(), new ExceptionLayout()))
                .BuildServiceProvider();

            MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(provider.GetRequiredService<ILoggerFactory>());

            using var connection = new MySqlConnection("server=localhost");

            connection.Open();
        }
    }
}