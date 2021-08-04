using Microsoft.Extensions.Logging;

namespace MS.DataAccess.Logging
{
    public class DataContextTraceLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _dataContextlogger;

        public DataContextTraceLoggerProvider(Serilog.ILogger seqLogger)
        {
            _dataContextlogger = new DataContextTraceLogger(seqLogger);
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            // one SEQ logger for all EF categories
            return _dataContextlogger;
        }
    }
}