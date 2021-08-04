using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MS.DataAccess.Logging
{
    /// <summary>
    /// Logger for DBContext 
    /// </summary>
    public class DataContextTraceLogger : ILogger
    {
        private Serilog.ILogger _logger;
        private readonly IEnumerable<int> _mainEventIdsForLogging;

        public DataContextTraceLogger(Serilog.ILogger logger)
        {
            _logger = logger;
            _mainEventIdsForLogging = new List<int> {20100, 20101, 20200, 20202, 20203, 20001, 10403};
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) && !_mainEventIdsForLogging.Contains(eventId.Id)) return;

            IReadOnlyList<KeyValuePair<string, object>> stateDataList = null;
            if (state is IReadOnlyList<KeyValuePair<string, object>>)
                stateDataList = (IReadOnlyList<KeyValuePair<string, object>>) state;

            if (formatter == null) throw new ArgumentNullException(nameof(formatter));
            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message)) return;

            switch (eventId.Id)
            {
                case 10403:
                    WriteDownContextInit(stateDataList);
                    break;
                case 20001:
                    WriteDownConnectionOpened(stateDataList);
                    break;
                case 20100:
                    WriteDownCommandExecuting(stateDataList);
                    break;
                case 20101:
                    WriteDownCommandExecuted(stateDataList);
                    break;
                case 20200:
                case 20202:
                case 20203:
                    WriteDownTransactions(message);
                    break;
            }
        }

        private void WriteDownConnectionOpened(IReadOnlyList<KeyValuePair<string, object>> stateDataList)
        {
            var dbName = stateDataList?.FirstOrDefault(p =>
                string.Equals(p.Key, "database", StringComparison.InvariantCultureIgnoreCase)).Value?.ToString();
            _logger = _logger.ForContext("DBName", dbName);
        }

        private void WriteDownContextInit(IReadOnlyList<KeyValuePair<string, object>> stateDataList)
        {
            var dataContextType = stateDataList?.FirstOrDefault(p =>
                    string.Equals(p.Key, "contextType", StringComparison.InvariantCultureIgnoreCase)).Value?
                .ToString();
            _logger = _logger.ForContext("DBContext", dataContextType);
        }

        private void WriteDownTransactions(string message)
        {
            _logger.Information(message);
        }

        private void WriteDownCommandExecuted(IReadOnlyList<KeyValuePair<string, object>> stateDataList)
        {
            var duration = stateDataList?.FirstOrDefault(p =>
                string.Equals(p.Key, "elapsed", StringComparison.InvariantCultureIgnoreCase)).Value?.ToString();
            var logger = _logger
                .ForContext("DBDuration", duration);
            logger.Information("Called DB: {DBContext}");
        }

        private void WriteDownCommandExecuting(IReadOnlyList<KeyValuePair<string, object>> stateDataList)
        {
            var sql = stateDataList?.FirstOrDefault(p =>
                    string.Equals(p.Key, "commandText", StringComparison.InvariantCultureIgnoreCase)).Value?
                .ToString();
            var parameters = stateDataList?.FirstOrDefault(p =>
                    string.Equals(p.Key, "parameters", StringComparison.InvariantCultureIgnoreCase)).Value?
                .ToString();
            var logger = _logger
                .ForContext("SQL", sql)
                .ForContext("SQL-parameters", parameters);
            logger.Information("Calling DB: {DBContext}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NullScope();
        }
    }
}