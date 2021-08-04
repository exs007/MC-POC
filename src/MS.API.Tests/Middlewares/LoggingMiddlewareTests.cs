using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using MS.API.Middlewares;
using MS.Core.Constants;
using MS.Core.Options;
using Serilog;
using Serilog.Events;
using Xunit;

namespace MS.API.Tests.Middlewares
{
    public class LoggingMiddlewareTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<HttpRequest> _httpRequestMock;
        private readonly Mock<HttpResponse> _httpResponseMock;
        private readonly IOptions<CommonOptions> _options;

        public LoggingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(p => p.ForContext<LoggingMiddleware>()).Returns(_loggerMock.Object);
            _loggerMock.Setup(p => p.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
                .Returns(_loggerMock.Object);
            _httpContextMock = new Mock<HttpContext>();
            _httpContextMock.Setup(p => p.Items).Returns(new Dictionary<object, object?>());
            _httpRequestMock = new Mock<HttpRequest>();
            _httpRequestMock.Setup(p => p.Headers).Returns(new HeaderDictionary());
            _httpContextMock.Setup(p => p.Request).Returns(_httpRequestMock.Object);
            _httpResponseMock = new Mock<HttpResponse>();
            _httpContextMock.Setup(p => p.Response).Returns(_httpResponseMock.Object);
            var serviceProviderMock = new Mock<IServiceProvider>();
            _httpContextMock.SetupGet(p => p.RequestServices).Returns(serviceProviderMock.Object);
            _options = Options.Create(new CommonOptions
            {
                Environment = "DEV"
            });
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenHeadMethod_InvokeAsync_NoInfoInLogs()
        {
            _httpRequestMock.SetupGet(p => p.Method).Returns("HEAD");

            var middleware = new LoggingMiddleware(context => Task.CompletedTask, _loggerMock.Object,
                _options, "TEST");
            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _loggerMock.Verify(p => p.Information(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            _loggerMock.Verify(p => p.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenNotHeadMethodStatusCode200_InvokeAsync_InfoAboutRequestLogged()
        {
            _httpRequestMock.SetupGet(p => p.Method).Returns("GET");
            _httpResponseMock.SetupGet(p => p.StatusCode).Returns((int) HttpStatusCode.OK);

            var middleware = new LoggingMiddleware(context => Task.CompletedTask, _loggerMock.Object,
                _options, "TEST");
            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _loggerMock.Verify(p => p.Write(LogEventLevel.Information, It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenNotHeadMethodStatusCodeBadRequest_InvokeAsync_InfoAboutRequestLogged()
        {
            _httpRequestMock.SetupGet(p => p.Method).Returns("GET");
            _httpResponseMock.SetupGet(p => p.StatusCode).Returns((int) HttpStatusCode.BadRequest);

            var middleware = new LoggingMiddleware(context => Task.CompletedTask, _loggerMock.Object,
                _options, "TEST");
            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _loggerMock.Verify(p => p.Write(LogEventLevel.Information, It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenNotHeadMethodStatusCodeInternalServerError_InvokeAsync_WarningAboutRequestLogged()
        {
            _httpRequestMock.SetupGet(p => p.Method).Returns("GET");
            _httpResponseMock.SetupGet(p => p.StatusCode).Returns((int) HttpStatusCode.InternalServerError);

            var middleware = new LoggingMiddleware(context => Task.CompletedTask, _loggerMock.Object,
                _options, "TEST");
            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _loggerMock.Verify(p => p.Write(LogEventLevel.Warning, It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenNotHeadAndExceptionThrown_InvokeAsync_WarningAboutRequestLoggedAndErrowRethrown()
        {
            _httpRequestMock.SetupGet(p => p.Method).Returns("GET");
            _httpResponseMock.SetupGet(p => p.StatusCode).Returns((int) HttpStatusCode.InternalServerError);

            var middleware = new LoggingMiddleware(context => throw new Exception(), _loggerMock.Object,
                _options, "TEST");
            var context = _httpContextMock.Object;
            await Assert.ThrowsAsync<Exception>(async () => await middleware.InvokeAsync(context));

            _loggerMock.Verify(p => p.Warning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenNotHeadMethodStatusCodeInternalServerError_InvokeAsync_OnlyLogWhiteListedHeaders()
        {
            _httpRequestMock.SetupGet(p => p.Method).Returns("POST");
            _httpResponseMock.SetupGet(p => p.StatusCode).Returns((int) HttpStatusCode.InternalServerError);
            var headers = new HeaderDictionary();
            headers.Add("Content-Type", "application/json");
            headers.Add("Content-Length", "123");
            headers.Add("User-Agent", "Test");
            headers.Add("Authorization ", "sensitive");
            _httpRequestMock.Setup(p => p.Headers).Returns(headers);

            var middleware = new LoggingMiddleware(context => Task.CompletedTask, _loggerMock.Object,
                _options, "TEST");
            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _loggerMock.Verify(p => p.Write(LogEventLevel.Warning, It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Once);
            _loggerMock.Verify(p => p.ForContext(It.IsAny<string>(), It.Is<Dictionary<string, string>>(p =>
                    p != null && !p.ContainsKey("Authorization") // no sensitive header logged
                              && p.ContainsKey("Content-Type") && p.ContainsKey("Content-Length") &&
                              p.ContainsKey("User-Agent")),
                It.IsAny<bool>()), Times.Once);
        }
    }
}