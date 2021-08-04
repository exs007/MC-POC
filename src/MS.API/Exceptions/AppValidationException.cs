using System;
using System.Collections.Generic;
using System.Net;
using MS.API.Models.Common;

namespace MS.API.Exceptions
{
    /// <summary>
    /// Base validation exception class, provides ability to customize returned status code
    /// </summary>
    public class AppValidationException : Exception
    {
        public IEnumerable<MessageInfo> Messages { get; }
        public HttpStatusCode StatusCode { get; }

        public AppValidationException(string message, HttpStatusCode? statusCode = null) : this(null, message,
            statusCode)
        {
        }

        public AppValidationException(string field, string message, HttpStatusCode? statusCode = null) :
            this(new List<MessageInfo> {new(field, message)}, statusCode)
        {
        }

        public AppValidationException(IEnumerable<MessageInfo> messages, HttpStatusCode? statusCode = null)
        {
            Messages = messages;
            StatusCode = statusCode ?? HttpStatusCode.BadRequest;
        }
    }
}