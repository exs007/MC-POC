using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using MS.Core.Constants;
using MS.DataAccess.Logging;
using Xunit;
using ILogger = Serilog.ILogger;

namespace MS.DataAccess.Tests.Logging
{
    public class DataContextTraceLoggerTests
    {
        private readonly DataContextTraceLogger _dataContextlogger;
        private readonly Mock<ILogger> _loggerMock;

        public DataContextTraceLoggerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(p => p.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
                .Returns(_loggerMock.Object);
            _dataContextlogger = new DataContextTraceLogger(_loggerMock.Object);
        }


        [Theory]
        [InlineData(20100)] // Command executing
        [InlineData(20101)] // Command executed
        [InlineData(20200)] // Open transaction
        [InlineData(20202)] // Committing transaction
        [InlineData(20203)] // Committed transaction
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenProvidedTargetEventId_Log_WriteInfoInLogs(int eventId)
        {
            _dataContextlogger.Log(LogLevel.Information, eventId, new List<KeyValuePair<string, object>>(), null,
                (state, exception) => "DB context info message");

            _loggerMock.Verify(p => p.Information(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenDBNameProvidedInStateDuringEvent20001_Log_LogContextIncludeDBName()
        {
            _dataContextlogger.Log(LogLevel.Information, 20001, new List<KeyValuePair<string, object>>
                {
                    new("database", "TestDB")
                },
                null, (state, exception) => "DB context info message");

            _loggerMock.Verify(p => p.ForContext("DBName", "TestDB", It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenContextTypeProvidedInStateDuringEvent10403_Log_LogContextIncludeDBName()
        {
            _dataContextlogger.Log(LogLevel.Information, 10403, new List<KeyValuePair<string, object>>
                {
                    new("contextType", "TestDBContext")
                },
                null, (state, exception) => "DB context info message");

            _loggerMock.Verify(p => p.ForContext("DBContext", "TestDBContext", It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenCallDurationProvidedInStateDuringEvent20101_Log_LogContextIncludeDBName()
        {
            _dataContextlogger.Log(LogLevel.Information, 20101, new List<KeyValuePair<string, object>>
                {
                    new("elapsed", "123")
                },
                null, (state, exception) => "DB context info message");

            _loggerMock.Verify(p => p.ForContext("DBDuration", "123", It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenCommandTextProvidedInStateDuringEvent20100_Log_LogContextIncludeDBName()
        {
            _dataContextlogger.Log(LogLevel.Information, 20100, new List<KeyValuePair<string, object>>
                {
                    new("commandText", "SQL statement")
                },
                null, (state, exception) => "DB context info message");

            _loggerMock.Verify(p => p.ForContext("SQL", "SQL statement", It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public void WhenSQLParamsProvidedInStateDuringEvent20100_Log_LogContextIncludeDBName()
        {
            _dataContextlogger.Log(LogLevel.Information, 20100, new List<KeyValuePair<string, object>>
                {
                    new("parameters", "SQL parameters")
                },
                null, (state, exception) => "DB context info message");

            _loggerMock.Verify(p => p.ForContext("SQL-parameters", "SQL parameters", It.IsAny<bool>()), Times.Once);
        }
    }
}