using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MS.API.Constants;
using MS.API.Exceptions;
using MS.API.Models.Common;
using Serilog;

namespace MS.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next,
            ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            ResponseInfo<object> result;
            switch (exception)
            {
                case AppValidationException appValidationException:
                    result = new ResponseInfo<object>(appValidationException.Messages);
                    statusCode = appValidationException.StatusCode;
                    break;
                case UnauthorizedException unauthEx:
                    _logger.Information(exception, "Unauthorized Exception");
                    result = new ResponseInfo<object>(unauthEx.Message);
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                default:
                    _logger.Error(exception, "Unhandled Exception");
                    result = new ResponseInfo<object>(MessageConstants.DEFAULT_ERROR_MESSAGE);
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.StatusCode = (int) statusCode;
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}