using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using MS.API.Exceptions;
using MS.API.Middlewares;
using MS.Core.Constants;
using Serilog;
using Xunit;

namespace MS.API.Tests.Middlewares
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<HttpResponse> _httpResponseMock;
        private readonly Mock<Stream> _bodyStreamMock;

        public ExceptionMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpContextMock = new Mock<HttpContext>();
            _httpResponseMock = new Mock<HttpResponse>();
            _httpContextMock.Setup(p => p.Response).Returns(_httpResponseMock.Object);
            var serviceProviderMock = new Mock<IServiceProvider>();
            _httpContextMock.SetupGet(p => p.RequestServices).Returns(serviceProviderMock.Object);
            _httpResponseMock.Setup(p => p.HttpContext).Returns(_httpContextMock.Object);
            _bodyStreamMock = new Mock<Stream>();
            _httpResponseMock.Setup(p => p.Body).Returns(_bodyStreamMock.Object);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenUnauthorizedExceptionThrown_InvokeAsync_StatusCodeToUnauthorizedAndInfoInLogs()
        {
            var middleware =
                new ExceptionMiddleware(context => throw new UnauthorizedException(), _loggerMock.Object);

            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _httpResponseMock.VerifySet(p => p.StatusCode = (int) HttpStatusCode.Unauthorized);
            _loggerMock.Verify(p => p.Information(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenNoExceptionThrown_InvokeAsync_NoInfoInLogsAndNotSetOfStatusCode()
        {
            var middleware = new ExceptionMiddleware(context => Task.CompletedTask, _loggerMock.Object);

            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _httpResponseMock.VerifySet(p => p.StatusCode = It.IsAny<int>(), Times.Never);
            _loggerMock.Verify(p => p.Information(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            _loggerMock.Verify(p => p.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenAppValidationExceptionThrown_InvokeAsync_StatusCodeBasedInfoFromExceptionAndNoInfoInLogs(
            HttpStatusCode expectedStatusCode)
        {
            var middleware =
                new ExceptionMiddleware(context => throw new AppValidationException("Error", expectedStatusCode),
                    _loggerMock.Object);

            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _httpResponseMock.VerifySet(p => p.StatusCode = (int) expectedStatusCode);
            _loggerMock.Verify(p => p.Information(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            _loggerMock.Verify(p => p.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenArgumentExceptionThrown_InvokeAsync_StatusCodeToInternalServerErrorAndErrorInLogs()
        {
            var middleware =
                new ExceptionMiddleware(context => throw new ArgumentException("Error"), _loggerMock.Object);

            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _httpResponseMock.VerifySet(p => p.StatusCode = (int) HttpStatusCode.InternalServerError);
            _loggerMock.Verify(p => p.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_UNIT)]
        public async Task WhenExceptionThrown_InvokeAsync_StatusCodeToInternalServerErrorAndErrorInLogs()
        {
            var middleware = new ExceptionMiddleware(context => throw new Exception("Error"), _loggerMock.Object);

            var context = _httpContextMock.Object;
            await middleware.InvokeAsync(context);

            _httpResponseMock.VerifySet(p => p.StatusCode = (int) HttpStatusCode.InternalServerError);
            _loggerMock.Verify(p => p.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}