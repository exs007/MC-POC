using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace MS.API.Services.Logging
{
    /// <summary>
    /// Configures SEQ server logger
    /// </summary>
    public class ConfigurationHelper
    {
        public static ILogger GetBaseConfiguration(IConfiguration configuration, string applicationName,
            Func<LoggerConfiguration, LoggerConfiguration> additionLoggerConfigurationAction = null)
        {
            var section = configuration.GetSection("Seq");
            var serverUrl = section.GetValue<string>("ServerUrl");
            var apiKey = section.GetValue<string>("ApiKey");
            if (string.IsNullOrWhiteSpace(serverUrl)) throw new ArgumentNullException(nameof(serverUrl));

            var config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .WriteTo.Seq(serverUrl, apiKey: apiKey);

            if (additionLoggerConfigurationAction != null) config = additionLoggerConfigurationAction(config);

            return config.CreateLogger();
        }
    }
}