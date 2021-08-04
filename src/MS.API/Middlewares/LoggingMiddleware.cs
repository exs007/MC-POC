using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using MS.API.Constants;
using MS.Core.Options;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace MS.API.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly string _applicationName;

        private readonly ICollection<string> _headerWhitelist = new List<string>
            {"Content-Type", "Content-Length", "User-Agent"};

        private readonly ILogger _logger;
        private readonly CommonOptions _commonOptions;
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next,
            ILogger logger,
            IOptions<CommonOptions> commonOptions,
            string applicationName)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger.ForContext<LoggingMiddleware>();
            _commonOptions = commonOptions.Value;
            _applicationName = applicationName;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            using (LogContext.PushProperty(LoggingConstants.CORRELATION_ID_LABEL, GetCorrelationId(httpContext)))
            using (LogContext.PushProperty(LoggingConstants.ENVIRONMENT_LABEL, _commonOptions.Environment))
            {
                var request = httpContext.Request;
                if (request.Method == "HEAD")
                {
                    await _next(httpContext);
                    return;
                }

                var logger = _logger.ForContext("ClientIP", httpContext.Connection?.RemoteIpAddress?.ToString())
                    .ForContext("HTTPMethod", request.Method)
                    .ForContext("ApplicationName", _applicationName)
                    .ForContext("URL", httpContext.Request.GetDisplayUrl());


                logger.Information("{ApplicationName} Request: {HTTPMethod} {URL}");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var responseFormat =
                    "{ApplicationName} Response: {HTTPMethod} {URL} responded {StatusCode} in {Elapsed:0.0000} ms";
                try
                {
                    await _next(httpContext);
                    stopwatch.Stop();

                    var statusCode = httpContext.Response?.StatusCode;
                    var level = statusCode > 499 ? LogEventLevel.Warning : LogEventLevel.Information;

                    logger = level == LogEventLevel.Warning ? LogForErrorContext(logger, httpContext) : logger;
                    logger.ForContext("Duration", stopwatch.ElapsedMilliseconds)
                        .Write(level, responseFormat, _applicationName, httpContext.Request.Method,
                            httpContext.Request.GetDisplayUrl(), statusCode, stopwatch.ElapsedMilliseconds);
                }

                catch (Exception ex)
                {
                    stopwatch.Stop();
                    LogForErrorContext(logger, httpContext)
                        .Warning(ex, responseFormat, _applicationName, httpContext.Request.Method,
                            httpContext.Request.GetDisplayUrl(), 500, stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }

        private string GetCorrelationId(HttpContext httpContext)
        {
            object value;
            if (httpContext.Items.TryGetValue(LoggingConstants.CORRELATION_ID_LABEL, out value)) return (string) value;

            var sGuid = Guid.NewGuid().ToString();
            httpContext.Items[LoggingConstants.CORRELATION_ID_LABEL] = sGuid;
            return sGuid;
        }

        private ILogger LogForErrorContext(ILogger logger, HttpContext httpContext)
        {
            var request = httpContext.Request;

            var loggedHeaders = request.Headers
                .Where(h => _headerWhitelist.Contains(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            var result = logger
                .ForContext("RequestHeaders", loggedHeaders, true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            return result;
        }
    }
}